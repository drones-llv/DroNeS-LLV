using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;

namespace Drones.Utils
{
    public static class MeshOptimizer
    {
        // Combines the parent and children meshes and delete children objects
        public static void CombineParentAndChildrenMeshes(Transform parent, bool editMode = true)
        {
            Matrix4x4 parentTransform;
            MeshFilter[] meshFilters;
            CombineInstance[] combine;
            Material[] materials = null;
            MeshCollider meshCollider;
            if (!IsParent(parent)) { return; }

            Mesh mainMesh = parent.gameObject.transform.GetComponent<MeshFilter>().mesh;
            parentTransform = parent.gameObject.transform.worldToLocalMatrix;
            meshFilters = parent.gameObject.GetComponentsInChildren<MeshFilter>();
            combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = parentTransform * meshFilters[i].transform.localToWorldMatrix;
            }
            MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();

            if (meshRenderer != null) { materials = meshRenderer.sharedMaterials; }

            Mesh batchedMesh = parent.gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            batchedMesh.CombineMeshes(combine);
            batchedMesh.name = parent.name;

            if (materials != null) { meshRenderer.materials = materials; }

            meshCollider = parent.gameObject.GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = parent.gameObject.GetComponent<MeshFilter>().sharedMesh;
            }

            parent.gameObject.transform.gameObject.SetActive(true);

            DeleteChildren(parent, editMode);
        }

        public static void DeleteChildren(Transform parent, bool editMode)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                if (editMode) { Object.DestroyImmediate(parent.GetChild(i).gameObject); }
                else { Object.Destroy(parent.GetChild(i).gameObject); }
            }
        }

        // Turn off unnecessary options in the renderer
        public static void OptimizeRenderer(Transform city)
        {
            Material[] materials = new Material[1];
            materials[0] = Resources.Load(Constants.buildingMaterialPath) as Material;
            Mesh mesh;
            MeshRenderer meshRenderer;

            foreach (Transform tile in city)
            {
                meshRenderer = tile.GetComponent<MeshRenderer>();
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.allowOcclusionWhenDynamic = true;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

                if (tile.childCount == 0) { continue;  }

                foreach (Transform i in tile)
                {
                    meshRenderer = i.GetComponent<MeshRenderer>();
                    mesh = i.GetComponent<MeshFilter>().mesh;
                    mesh.SetTriangles(mesh.triangles, 0);
                    mesh.subMeshCount = 1;
                    meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    if (i.name.Substring(0,4) == "Road")
                    {
                        i.GetComponent<MeshCollider>().convex = false;
                        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                        meshRenderer.receiveShadows = false;
                        Material[] roadmat = new Material[1];
                        roadmat[0] = meshRenderer.sharedMaterials[0];
                        meshRenderer.materials = roadmat;
                    }
                    else
                    {
                        i.GetComponent<MeshCollider>().convex = true;
                        i.gameObject.layer = LayerMask.NameToLayer("BuildingCollider"); // Set layer to buildings
                        meshRenderer.materials = materials;
                    }
                }
            }
        }

        // Makes grouping buildings easier, typically similar names = same group
        public static void SortChildrenByName(Transform parent)
        {
            var children = parent.GetComponentsInChildren<Transform>().ToList();
            children.Sort((Transform t1, Transform t2) => {
                return string.Compare(t1.name, t2.name, System.StringComparison.Ordinal);
            });

            for (int i = 0; i < children.Count; i++)
                children[i].SetSiblingIndex(i);
        }

        // Groups building structures into city blocks
        public static void GroupAllByBlocks(Transform city)
        {
            List<Transform> tallBuildings = new List<Transform>();
            List<Transform> shortBuildings = new List<Transform>();

            GameObject road = null;
            foreach (Transform tile in city)
            {
                /* Preprocess: putting all the buildings in a list
                since hierarchy changes as we go through the loop */
                foreach (Transform i in tile)
                {
                    if (IsTall(i))
                    {
                        tallBuildings.Add(i);
                    }
                    else if (i.name.Substring(0,4) == "Road") { road = i.gameObject; }
                    else { shortBuildings.Add(i); }
                }

                GroupByBlock(tallBuildings, road, tile);
                GroupByBlock(shortBuildings, road, tile);
            }
        }

        // Do SplitBlock on the whole city 
        public static void SplitAllBlocks(Transform city)
        {
            foreach (Transform tile in city)
            {
                for (int i = tile.childCount - 1; i >= 0; i--)
                {
                    Transform block = tile.GetChild(i);
                    if (block.childCount > 0) { SplitBlock(block); }
                }
            }
        }

        // Sorts the hierarchy so that parents will be the shortest structure and splits children to limit vertex count
        public static void SortHeirarchy(Transform city)
        {
            foreach (Transform tile in city)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform building = tile.GetChild(i);
                    if (building.childCount != 0)
                    {
                        Transform parent = SortBuildingComponents(building);
                        LimitVertex(parent);
                    }
                }

            }
        }

        // Checks if the object name begins with "Tall" //TODO make this more generic?
        public static bool IsTall(Transform building)
        {
            return building.name.Substring(0, 4) == "Tall";
        }

        public static bool IsShort(Transform building)
        {
            return building.name.Substring(0, 5) == "Short";
        }

        public static bool IsSLOD(Transform building)
        {
            return building.name.Substring(0, 4) == "SLOD";
        }

        public static bool IsTLOD(Transform building)
        {
            return building.name.Substring(0, 4) == "TLOD";
        }

        public static void GroupByTile(Transform tile)
        {
            Transform tallParent = null;
            Transform shortParent = null;
            List<Transform> newParents = new List<Transform>();

            void addNonNull(Transform x) { if (x != null) { newParents.Add(x); } }

            for (int i = 0; i < tile.childCount; i++)
            {
                Transform building = tile.GetChild(i);
                if (building.childCount > 0)
                {
                    building.GetChild(0).SetParent(building.parent);
                }
            }

            for (int i = tile.childCount - 1; i >= 0; i--)
            {
                Transform building = tile.GetChild(i);
                if (IsTall(building))
                {
                    if (tallParent == null)
                    {
                        tallParent = building;
                        continue;
                    }
                    building.SetParent(tallParent);
                }
                else if (IsShort(building))
                {
                    if (shortParent == null)
                    {
                        shortParent = building;
                        continue;
                    }
                    building.SetParent(shortParent);
                } 
            }
            if (tallParent != null)
            {
                addNonNull(LimitVertex(tallParent));
                CombineParentAndChildrenMeshes(tallParent);
            }
            if (shortParent != null)
            {
                addNonNull(LimitVertex(shortParent));
                CombineParentAndChildrenMeshes(shortParent);
            }
            for (int i = 0; i < newParents.Count; i++)
            {
                addNonNull(LimitVertex(newParents[i]));
                CombineParentAndChildrenMeshes(newParents[i]);
            }

        }

        // Checks if objects are in the same city block
        private static bool InBlock(GameObject blockParent, GameObject i, GameObject road)
        {
            var meshO = blockParent.GetComponent<MeshRenderer>();
            var meshI = i.GetComponent<MeshRenderer>();
            var roadMesh = road.GetComponent<MeshCollider>();
            float roadLevel = road.GetComponent<MeshRenderer>().bounds.center.y;
            Bounds oBounds = meshO.bounds;
            Bounds iBounds = meshI.bounds;
            Vector3 dir;

            oBounds.center = new Vector3(oBounds.center.x, roadLevel, oBounds.center.z);
            iBounds.center = new Vector3(iBounds.center.x, roadLevel, iBounds.center.z);
            dir = (iBounds.center - oBounds.center).normalized;

            // Horizontal distance between centres
            float dist = (iBounds.center - oBounds.center).magnitude;

            return dist < 4.0f || !roadMesh.Raycast(new Ray(oBounds.center, dir), out RaycastHit hit, dist);
        }

        // Groups buildings in the list by city blocks
        private static void GroupByBlock(List<Transform> buildings, GameObject road, Transform tile)
        {
            if (buildings.Count > 1 && road != null)
            {
                foreach (Transform i in buildings)
                {
                    if (i.parent != tile) { continue; }

                    foreach (Transform j in buildings)
                    {
                        if (j != i && InBlock(i.gameObject, j.gameObject, road))
                        {
                            if (j.parent == tile)
                            {
                                j.SetParent(i, true);
                                while (j.childCount > 0) { j.GetChild(0).SetParent(i, true); }
                            }
                            else if (j.parent != i)
                            {
                                Transform k = j.parent;
                                k.SetParent(i, true);
                                while (k.childCount > 0) { k.GetChild(0).SetParent(i, true); }
                            }
                        }
                    }
                }
            }
            buildings.Clear();
        }

        // Does building i and building o overlap (horizontally)
        private static bool IsAreaOverlap(GameObject o, GameObject i, float dist)
        {
            var meshO = o.GetComponent<MeshRenderer>();
            var meshI = i.GetComponent<MeshRenderer>();

            Vector3 iTranslate = new Vector3(0, meshO.bounds.center.y - meshI.bounds.center.y, 0);
            // Move object i so that o & i have the same centre level
            i.transform.position += iTranslate;
            Vector3 oFace = o.GetComponent<MeshCollider>().ClosestPoint(meshI.bounds.center);
            Vector3 iFace = i.GetComponent<MeshCollider>().ClosestPoint(meshO.bounds.center);
            Vector3 c2c = meshO.bounds.center - meshI.bounds.center;
            i.transform.position -= iTranslate;
            // dist is the maximum distance between buildings to be considered a single building
            if (Vector3.Dot(oFace - iFace, c2c) > 0)
            {
                return Vector3.Magnitude(oFace - iFace) < dist;
            }

            return true;
        }

        // If there are multiple tall buildings in city block split them up
        private static void SplitBlock(Transform block)
        {
            float dist;
            if (IsTall(block)) { dist = 2.0f; }
            else if (IsShort(block)) { dist = 5.0f; }
            else { return; }

            // A group represents a set of objects which potentially belong to the same building
            List<List<Transform>> allGroups = new List<List<Transform>>();
            List<List<Transform>> intersects = new List<List<Transform>>();
            Transform currentChild;
            allGroups.Add(new List<Transform>());
            allGroups[0].Add(block);
            for (int i = block.childCount - 1; i >= 0; i--)
            {
                currentChild = block.GetChild(i);
                // For each building
                foreach (List<Transform> group in allGroups)
                {
                    // Check if the current object is part of a building
                    foreach (Transform member in group)
                    {
                        if (IsAreaOverlap(member.gameObject, currentChild.gameObject, dist))
                        {
                            intersects.Add(group); // The groups that intersect with current child
                            break;
                        }
                    }
                }

                /* If the current object is intersects with multiple buildings then these buildings are the same */
                while (intersects.Count > 1)
                {
                    MergeLists(intersects[0], intersects[1]);
                    allGroups.Remove(intersects[1]);
                    intersects.RemoveAt(1);
                }

                /* If the current object is not part of any known buildings create a new group */
                if (intersects.Count == 0)
                {
                    allGroups.Add(new List<Transform>());
                    allGroups[allGroups.Count - 1].Add(currentChild);
                }
                else
                {
                    intersects[0].Add(currentChild);
                }

                intersects.Clear();
            }
            /* If there are multiple buildings in the block, rearrange the tile hierarchy */
            if (allGroups.Count > 1)
            {
                foreach (List<Transform> list in allGroups)
                {
                    // Set every other element to be a child of the first element
                    for (int i = 1; i < list.Count; i++) { list[i].SetParent(list[0], true); }
                    // Set the first element to be a child of the tile (i.e. the block's parent)
                    list[0].SetParent(block.parent, true);
                }
            }

        }

        // Sets the parent of a building to be the shortest structure and arranges the children in ascending height
        private static Transform SortBuildingComponents(Transform building)
        {
            Transform shortest = building;
            int HeightCompare(Transform t1, Transform t2)
            {
                float val = t1.GetComponent<MeshRenderer>().bounds.center.y;
                val -= t2.GetComponent<MeshRenderer>().bounds.center.y;
                if (val < Constants.EPSILON) return -1;
                return 1;
            }

            var children = building.GetComponentsInChildren<Transform>().ToList();
            children.Add(building);
            children.Sort(HeightCompare);
            shortest = children[0];
            children.RemoveAt(0);
            shortest.SetParent(building.parent, true);
            for (int i = 0; i < children.Count; i++)
                children[i].SetSiblingIndex(i);

            return shortest;
        }

        // Limits the total vertex count of a building by splitting up objects in the hierarchy
        private static Transform LimitVertex(Transform parent)
        {
            int vertices;
            MeshFilter meshFilter;
            Transform newParent = null;
            vertices = parent.GetComponent<MeshFilter>().sharedMesh.vertexCount;
            for (int i = 0; i < parent.childCount; i++)
            {
                meshFilter = parent.GetChild(i).GetComponent<MeshFilter>();
                vertices += meshFilter.sharedMesh.vertexCount;
                if (vertices > 65000) 
                { 
                    newParent = SplitChildren(parent, i); 
                }
            }
            return newParent;
        }

        // Splits the children of building from index onwards into a new parent in the hierarchy
        private static Transform SplitChildren(Transform building, int index)
        {
            Transform newParent = building.GetChild(index);
            int i = index + 1;
            while (i < building.childCount) { building.GetChild(i).SetParent(building.GetChild(index), true); }
            building.GetChild(index).SetParent(building.parent, true);

            return newParent;
        }

        // Adds element of list2 into list1 and clears list2
        private static void MergeLists<T>(List<T> list1, List<T> list2)
        {
            foreach (T element in list2) { list1.Add(element); }

            list2.Clear();
        }

        public static bool IsParent(Transform parent)
        {
            if (parent.childCount == 0) return false;

            return true;
        }



    }
}
