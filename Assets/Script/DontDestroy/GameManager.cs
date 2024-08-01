using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<SongDetail> songList = new List<SongDetail> ();
    public int selectedIndex = 0;
    public int selectedDiff = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //TODO: Read Last selected
        selectedIndex = 0;
        songList = SongLoader.ScanMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
