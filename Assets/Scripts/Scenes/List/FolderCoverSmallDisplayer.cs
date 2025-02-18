using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.List
{
    internal class FolderCoverSmallDisplayer: CoverSmallDisplayer
    {
        [SerializeField]
        TextMeshProUGUI _folderText;
        [SerializeField]
        GameObject _icon;

        SongCollection _boundCollection = SongCollection.Empty("Undefined");

        private void Start()
        {
            if (IsOnline)
                _icon.gameObject.SetActive(true);
        }
        internal void SetCollection(SongCollection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            _boundCollection = collection;
            _folderText.text = collection.Name;
        }
    }
}
