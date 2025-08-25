using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    internal class FolderCoverSmallDisplayer: CoverSmallDisplayer
    {
        [SerializeField]
        TextMeshProUGUI _folderText;
        [SerializeField]
        GameObject _icon;
        [SerializeField]
        GameObject _danIcon;

        SongCollection _boundCollection = SongCollection.Empty("Undefined");

        internal void SetCollection(SongCollection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            _boundCollection = collection;
            _folderText.text = collection.Name;
            if(collection.IsOnline)
            {
                _icon.gameObject.SetActive(true);
            }
            else
            {
                _icon.gameObject.SetActive(false);
            }
            if (collection.DanInfo != null) { 
                _danIcon.gameObject.SetActive(true);
            }
            else
            {
                _danIcon.gameObject.SetActive(false);
            }
        }
    }
}
