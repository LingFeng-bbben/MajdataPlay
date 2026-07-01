using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CollectionDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("onlineCollectionIcon")]
        Sprite _onlineCollectionIcon;
        [SerializeField]
        [FormerlySerializedAs("folderCollectionIcon")]
        Sprite _folderCollectionIcon;
        [SerializeField]
        [FormerlySerializedAs("favoriteCollectionIcon")]
        Sprite _favoriteCollectionIcon;
        [SerializeField]
        [FormerlySerializedAs("onlineFavoriteCollectionIcon")]
        Sprite _onlineFavoriteCollectionIcon;

        [SerializeField]
        [FormerlySerializedAs("iconDisplayer")]
        Image _iconDisplayer;

        [SerializeField]
        [FormerlySerializedAs("nameDisplayer")]
        TextMeshProUGUI _nameDisplayer;

        SongCollection? _bindingCollection;


        internal void SetCollection(SongCollection collection)
        {
            if(collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            _bindingCollection = collection;

            _nameDisplayer.text = collection.Name;
        }
    }
}
