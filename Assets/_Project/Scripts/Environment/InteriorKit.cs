using UnityEngine;

namespace TheLightHouse
{
    [SelectionBase]
    public sealed class InteriorKit : MonoBehaviour
    {
        public enum PieceType
        {
            Wall,
            FloorSlab,
            DoorFrame,
            StairFlight,
            Landing,
            Handrail,
            WindowFrame,
            VaultSegment
        }

        [SerializeField] private PieceType pieceType;
        [SerializeField] private Vector3 gridSize = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private InteriorSettings settings;

        public PieceType Type => pieceType;
        public Vector3 GridSize => gridSize;
        public InteriorSettings Settings => settings;

        public void Configure(PieceType type, InteriorSettings interiorSettings)
        {
            pieceType = type;
            settings = interiorSettings;
        }

        public void SnapToGrid()
        {
            var p = transform.localPosition;
            p.x = Mathf.Round(p.x / gridSize.x) * gridSize.x;
            p.y = Mathf.Round(p.y / gridSize.y) * gridSize.y;
            p.z = Mathf.Round(p.z / gridSize.z) * gridSize.z;
            transform.localPosition = p;
        }
    }
}
