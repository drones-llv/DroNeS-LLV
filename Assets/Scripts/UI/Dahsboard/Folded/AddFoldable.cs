using UnityEngine;

namespace Drones.UI
{
    public class AddFoldable : FoldableMenu
    {
        protected override void Start()
        {
            Buttons[0].onClick.AddListener(MakeHub);
            Buttons[1].onClick.AddListener(MakeNFZ);
            base.Start();
        }

        void MakeNFZ()
        {
            NoFlyZone nfz = NoFlyZone.New();
            var pos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            var pos2 = nfz.transform.position;
            pos = Selectable.Cam.ScreenToWorldPoint(pos);
            pos2.x = pos.x;
            pos2.y = nfz.transform.localScale.y / 2 + 0.1f;
            pos2.z = pos.z;
            nfz.transform.position = pos2;
        }

        void MakeHub()
        {
            Hub hub = Hub.New();
            var pos = new Vector3(Screen.width/2, Screen.height/2, 0);
            var pos2 = hub.transform.position;
            pos = Selectable.Cam.ScreenToWorldPoint(pos);
            pos2.x = pos.x + Random.Range(-1, 1);
            pos2.y = 500;
            pos2.z = pos.z + Random.Range(-1, 1);
            hub.transform.position = pos2;
        }
    }
}
