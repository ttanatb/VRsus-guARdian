using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlockPlacer : NetworkBehaviour
{
    #region SyncListStruct
    public struct Block
    {
        public Vector3 position;
        public Vector3 scale;

        public Block(Vector3 position, Vector3 scale)
        {
            this.position = position;
            this.scale = scale;
        }

        //public void Update(Vector3 position, float rotation, Vector3 scale)
        //{
        //    this.position = position;
        //    this.rotation = rotation;
        //    this.scale = scale;
        //}
    }

    public class BlockSync : SyncListStruct<Block> { }
    #endregion


    BlockSync m_ARPlane = new BlockSync();
    bool isPlacing;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
