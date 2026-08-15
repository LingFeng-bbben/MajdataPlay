using System.Collections.Generic;

namespace PDNWrapper
{
    internal static class Layer
    {
        public static BitmapLayer CreateBackgroundLayer(int w, int h)
        {
            return new BitmapLayer(new Rectangle(0, 0, w, h));
        }
    }

    internal class BitmapLayer
    {
        public int LayerID { get; set; }
        public bool IsGroup { get; set; }
        public BitmapLayer ParentLayer { get; set; }
        public IEnumerable<BitmapLayer> ChildLayer => m_ChildLayers;
        public string Name { get; set; }
        public byte Opacity { get; set; }
        public bool Visible { get; set; }
        public LayerBlendMode BlendMode { get; set; }
        public Surface Surface { get; }
        public Rectangle documentRect { get; private set; }
        public Rectangle localRect { get; }

        public bool IsEmpty => !(documentRect.Width > 0 && documentRect.Height > 0);

        readonly List<BitmapLayer> m_ChildLayers;

        public void Dispose()
        {
            Surface.Dispose();
            foreach (BitmapLayer layer in m_ChildLayers)
                layer.Dispose();
        }

        public BitmapLayer(Rectangle documentRect)
        {
            localRect = new Rectangle(0, 0, documentRect.Width, documentRect.Height);
            this.documentRect = documentRect;

            Surface = new Surface(localRect.Width, localRect.Height);

            m_ChildLayers = new List<BitmapLayer>();
            IsGroup = false;
        }

        public void AddChildLayer(BitmapLayer c)
        {
            m_ChildLayers.Add(c);
            Rectangle bound = c.documentRect;
            foreach (BitmapLayer child in ChildLayer)
            {
                bound.Y = bound.Y > child.documentRect.Y ? child.documentRect.Y : bound.Y;
                bound.X = bound.X > child.documentRect.X ? child.documentRect.X : bound.X;
                bound.Width = bound.Right < child.documentRect.Right ? child.documentRect.Right - bound.X : bound.Width;
                bound.Height = bound.Bottom < child.documentRect.Bottom ? child.documentRect.Bottom - bound.Y : bound.Height;
            }

            documentRect = bound;
        }

        public bool ShouldImport(bool importHiddenLayer, bool parentGroupVisible = true)
        {
            bool layerVisible = Visible && parentGroupVisible;
            if (!IsGroup)
            {
                return (importHiddenLayer || layerVisible) && !IsEmpty;
            }

            foreach (BitmapLayer child in ChildLayer)
            {
                if (child.ShouldImport(importHiddenLayer, layerVisible))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
