using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleManager : MonoBehaviour
{
    public Material waterMaterial;
    public ComputeShader rippleCompute;
    public RenderTexture NState, Nm1State, Np1State;
    public Vector2Int Resolution;

    public Vector3 effect; // x coord, y coord, strength
    public float dispersion = 0.98f;

    // Start is called before the first frame update
    void Start()
    {
        InitializeTexture(ref NState);
        InitializeTexture(ref Nm1State);
        InitializeTexture(ref Np1State);

        waterMaterial.mainTexture = NState;
    }

    void InitializeTexture(ref RenderTexture tex)
    {
        tex = new RenderTexture(Resolution.x, Resolution.y, 1, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SNorm);
        tex.enableRandomWrite = true;
        tex.Create();
    }

    // Update is called once per frame
    void Update()
    {

        Graphics.CopyTexture(NState, Nm1State);
        Graphics.CopyTexture(Np1State, NState);

        rippleCompute.SetTexture(0, "NState", NState);
        rippleCompute.SetTexture(0, "Nm1State", Nm1State);
        rippleCompute.SetTexture(0, "Np1State", Np1State);
        rippleCompute.SetVector("effect", effect);
        rippleCompute.SetVector("resolution", new Vector2 (Resolution.x, Resolution.y));
        rippleCompute.SetFloat("dispersion", dispersion);
        rippleCompute.Dispatch(0, Resolution.x / 8, Resolution.y / 8, 1);
    }
}
