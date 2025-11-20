using System.Collections.Generic;
using UnityEngine;

namespace Jigsaw
{
    public class JigsawPiece
    {
        private const float SNAP_DISTANCE = 20f;

        public JigsawPiece(JigsawContainer owner, int x, int y)
        {
            this.x = x; this.y = y; 
            jigsaw = owner;
            group = new(this);
            pos = new Vector2(Random.value * Futile.screen.pixelWidth, Random.value * Futile.screen.pixelHeight);
        }

        public int x, y;
        public JigsawContainer jigsaw;
        public Group group;
        public Vector2 pos;
        public FSprite sprite;

        public Vector2 NormalizedSize => new(1f / jigsaw.Width, 1f / jigsaw.Height);
        public Vector2 RectSize => new Vector2(Futile.screen.pixelWidth, Futile.screen.pixelHeight) * NormalizedSize;

        public bool MouseIntersecting => new Rect(pos - RectSize / 2f, RectSize).Contains((Vector2)Futile.mousePosition);

        public bool CornerPiece => (x == 0 && y == 0) || (x == 0 && y == jigsaw.Height - 1)
            || (x == jigsaw.Width - 1 && y == 0) || (x == jigsaw.Width - 1 && y == jigsaw.Height - 1);
        private Vector2 CornerPosition()
        {
            float vx = Futile.screen.pixelWidth / 2f;
            float vy = Futile.screen.pixelHeight / 2f;

            if (x == 0) vx = RectSize.x / 2;
            if (y == 0) vy = RectSize.y / 2;
            if (x == jigsaw.Width - 1) vx = RectSize.x * (jigsaw.Width - 0.5f);
            if (y == jigsaw.Height - 1) vy = RectSize.y * (jigsaw.Height - 0.5f);
            
            return new Vector2(vx, vy - 1f); // I'm not sure why but it keeps being 1 pixel offset vertically
        }

        public void InitializeSprite()
        {
            sprite?.RemoveFromContainer();
            sprite = new FSprite("Futile_White")
            {
                shader = Shaders.PuzzlePiece,
                color = new Color(x / 255f, y / 255f, 0f, 1f),
                scaleX = RectSize.x / 8f,
                scaleY = RectSize.y / 8f,
            };
            sprite.SetPosition(pos);
        }

        public void Move(Vector2 amount)
        {
            // This will include ourselves
            foreach (var piece in group.pieces)
            {
                piece.pos += amount;
                piece.sprite.SetPosition(piece.pos + Vector2.one * 0.01f);
            }
        }

        public void Drop()
        {
            // This will include ourselves
            List<JigsawPiece> piecesTemp = [.. group.pieces];
            foreach (var piece in piecesTemp)
            {
                piece.DropInternal();
            }

            // Snap to corners if needed
            foreach (var piece in group.pieces)
            {
                piece.CornerSnap();
            }
        }

        private void DropInternal()
        {
            for (int i = 0; i < jigsaw.Width; i++)
            {
                for (int j = 0; j < jigsaw.Height; j++)
                {
                    var other = jigsaw.pieces[i, j];
                    if (!group.Contains(other))
                    {
                        if (x + 1 == other.x && y == other.y && Vector2.Distance(pos + RectSize * Vector2.right, other.pos) < SNAP_DISTANCE)
                        {
                            Merge(other.pos - (pos + RectSize * Vector2.right), other, other.group);
                        }
                        else if (x - 1 == other.x && y == other.y && Vector2.Distance(pos + RectSize * Vector2.left, other.pos) < SNAP_DISTANCE)
                        {
                            Merge(other.pos - (pos + RectSize * Vector2.left), other, other.group);
                        }
                        else if (x == other.x && y + 1 == other.y && Vector2.Distance(pos + RectSize * Vector2.up, other.pos) < SNAP_DISTANCE)
                        {
                            Merge(other.pos - (pos + RectSize * Vector2.up), other, other.group);
                        }
                        else if (x == other.x && y - 1 == other.y && Vector2.Distance(pos + RectSize * Vector2.down, other.pos) < SNAP_DISTANCE)
                        {
                            Merge(other.pos - (pos + RectSize * Vector2.down), other, other.group);
                        }
                    }
                }
            }

            CornerSnap();
        }

        private void Merge(Vector2 idealMove, JigsawPiece otherPiece, Group otherGroup)
        {
            if (otherGroup.Count > group.Count)
            {
                Move(idealMove);
                otherGroup.Merge(group);
            }
            else
            {
                otherPiece.Move(-idealMove);
                group.Merge(otherGroup);
            }
            
        }

        private void CornerSnap()
        {
            if (CornerPiece && Vector2.Distance(pos, CornerPosition()) < SNAP_DISTANCE)
            {
                Move(CornerPosition() - pos);
            }
        }

        public class Group
        {
            public HashSet<JigsawPiece> pieces;

            public Group(JigsawPiece initial)
            {
                pieces = [];
                Add(initial);
            }

            public void Add(JigsawPiece piece)
            {
                pieces.Add(piece);
                piece.group = this;
            }

            public bool Contains(JigsawPiece piece) => pieces.Contains(piece);

            public int Count => pieces.Count;

            public void Merge(Group otherGroup)
            {
                pieces.UnionWith(otherGroup.pieces);
                foreach (var otherMember in otherGroup.pieces)
                {
                    otherMember.group = this;
                }
            }
        }
    }
}
