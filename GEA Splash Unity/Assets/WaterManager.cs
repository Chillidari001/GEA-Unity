using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public float waveHeight = 0.05f;
    public float waveFrequency = 0.77f;
    public float waveSpeed = 2f;
    public GameObject water;

    Material waterMaterial;
    //Texture2D displacementWaves;


    void Start()
    {
        setVariables();
    }

    void setVariables()
    {
        waterMaterial = water.GetComponent<Renderer>().sharedMaterial;
        //displacementWaves = (Texture2D)waterMaterial.GetTexture("__di")
    }

    /*public float waterHeightAtPosition(Vector3 position)
    {

    }*/
}