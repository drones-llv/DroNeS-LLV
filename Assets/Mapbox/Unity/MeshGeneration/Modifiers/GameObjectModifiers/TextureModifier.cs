namespace Mapbox.Unity.MeshGeneration.Modifiers
{
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Components;
	using Mapbox.Unity.MeshGeneration.Data;
	using System;

	/// <summary>
	/// Texture Modifier is a basic modifier which simply adds a TextureSelector script to the features.
	/// Logic is all pushed into this TextureSelector mono behaviour to make it's easier to change it in runtime.
	/// </summary>

	[Obsolete("Texture Modifier is obsolete. Please use Material Modifier.")]
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Texture Modifier")]
    public class TextureModifier : GameObjectModifier
    {
        [SerializeField]
        public bool _textureTop;
        [SerializeField]
        public bool _useSatelliteTexture;
        [SerializeField]
        public Material[] _topMaterials;

		[SerializeField]
        public bool _textureSides;
        [SerializeField]
        public Material[] _sideMaterials;

		public override void Run(VectorEntity ve, UnityTile tile)
        {
			var _meshRenderer = ve.MeshRenderer;
			if (_textureSides && _sideMaterials.Length > 0)
			{
				_meshRenderer.materials = new Material[2]
				{
				_topMaterials[UnityEngine.Random.Range(0, _topMaterials.Length)],
				_sideMaterials[UnityEngine.Random.Range(0, _sideMaterials.Length)]
				};
			}
			else if (_textureTop)
			{
				_meshRenderer.materials = new Material[1]
			   {
				_topMaterials[UnityEngine.Random.Range(0, _topMaterials.Length)]
			   };
			}

			if (_useSatelliteTexture)
			{
				_meshRenderer.materials[0].mainTexture = tile.GetRasterData();
				_meshRenderer.materials[0].mainTextureScale = new Vector2(1f, 1f);
			}
        }
    }
}
