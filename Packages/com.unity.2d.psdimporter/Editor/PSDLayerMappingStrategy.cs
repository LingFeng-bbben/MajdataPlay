using System.Collections;
using System.Collections.Generic;
using PDNWrapper;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    internal interface IPSDLayerMappingStrategyComparable
    {
        int layerID
        {
            get;
        }

        string name
        {
            get;
        }

        bool isGroup
        {
            get;
        }
    }

    internal interface IPSDLayerMappingStrategy
    {
        bool Compare(IPSDLayerMappingStrategyComparable a, IPSDLayerMappingStrategyComparable b);
        bool Compare(IPSDLayerMappingStrategyComparable a, BitmapLayer b);
        string LayersUnique(IEnumerable<IPSDLayerMappingStrategyComparable> layers);
    }

    internal abstract class LayerMappingStrategy<T> : IPSDLayerMappingStrategy
    {
        string m_DuplicatedStringError = L10n.Tr("The following layers have duplicated identifier.");
        protected abstract T GetID(IPSDLayerMappingStrategyComparable layer);
        protected abstract T GetID(BitmapLayer layer);

        protected virtual bool IsGroup(IPSDLayerMappingStrategyComparable layer)
        {
            return layer.isGroup;
        }

        protected virtual bool IsGroup(BitmapLayer layer)
        {
            return layer.IsGroup;
        }

        public bool Compare(IPSDLayerMappingStrategyComparable x, BitmapLayer y)
        {
            return Comparer<T>.Default.Compare(GetID(x), GetID(y)) == 0 && IsGroup(x) == IsGroup(y);
        }

        public bool Compare(IPSDLayerMappingStrategyComparable x, IPSDLayerMappingStrategyComparable y)
        {
            return Comparer<T>.Default.Compare(GetID(x), GetID(y)) == 0 && IsGroup(x) == IsGroup(y);
        }

        public string LayersUnique(IEnumerable<IPSDLayerMappingStrategyComparable> layers)
        {
            HashSet<T> layerNameHash = new HashSet<T>();
            HashSet<T> layerGroupHash = new HashSet<T>();
            return LayersUnique(layers, layerNameHash, layerGroupHash);
        }

        string LayersUnique(IEnumerable<IPSDLayerMappingStrategyComparable> layers, HashSet<T> layerNameHash, HashSet<T> layerGroupHash)
        {
            List<string> duplicateLayerName = new List<string>();
            string duplicatedStringError = null;
            foreach (IPSDLayerMappingStrategyComparable layer in layers)
            {
                T id = GetID(layer);
                HashSet<T> hash = layer.isGroup ? layerGroupHash : layerNameHash;
                if (hash.Contains(id))
                    duplicateLayerName.Add(layer.name);
                else
                    hash.Add(id);
            }

            if (duplicateLayerName.Count > 0)
            {
                duplicatedStringError = m_DuplicatedStringError + "\n";
                duplicatedStringError += string.Join(", ", duplicateLayerName);
            }
            return duplicatedStringError;
        }
    }

    internal class LayerMappingUseLayerName : LayerMappingStrategy<string>
    {
        protected override string GetID(IPSDLayerMappingStrategyComparable x)
        {
            return x.name.ToLower();
        }

        protected override string GetID(BitmapLayer x)
        {
            return x.Name.ToLower();
        }
    }

    internal class LayerMappingUseLayerNameCaseSensitive : LayerMappingStrategy<string>
    {
        protected override string GetID(IPSDLayerMappingStrategyComparable x)
        {
            return x.name;
        }

        protected override string GetID(BitmapLayer x)
        {
            return x.Name;
        }
    }

    internal class LayerMappingUserLayerID : LayerMappingStrategy<int>
    {
        protected override int GetID(IPSDLayerMappingStrategyComparable x)
        {
            return x.layerID;
        }

        protected override int GetID(BitmapLayer x)
        {
            return x.LayerID;
        }
    }
}

