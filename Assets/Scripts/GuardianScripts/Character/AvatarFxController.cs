using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SensFx {Explosion, Integration}
public class AvatarFxController : MonoBehaviour
{
    [SerializeField] Transform ModelFx;
    Transform[] allModelParts;
    Vector3[] allPosIni;
    Vector3[] allRotIni;
    Vector3[] allDirExplosion;
    Material[] allMatParts;

    [Header ("Dissolve settings (ne pas toucher normalement):")]
    [SerializeField] float valMax = 1.1f;
    [SerializeField] float valMin = -0.1f;
    [Header ("Bump settings :")]
    [SerializeField] [ColorUsage (false, true)] Color colorBump;
    [Range (0,1)]
    [SerializeField] float widthOutlineBump;
    Color iniColorMat;
    float iniFloatMat;
    [Range (0.01f,1)]
    [SerializeField] float speedBump;
    [Header("Shader properties name :")]
    [SerializeField] string AmountProperty = "_Amount";
    [SerializeField] string DirectionProperty = "_DirectionDissolve";
    [SerializeField] string OutilneColorProperty = "_OutlineColor";
    [SerializeField] string OutilneWidthProperty = "_OutlineWidth";

    [Header ("Explosion/Reconstruction settings :")]
    [SerializeField] float speedAnimation = 1;
    [SerializeField] float delay;
    [SerializeField] Transform RefCenterExplosion;
    [Header ("Settings to custom explosion (range):")]
    [Range (1,10)]
    [SerializeField] float GlobalAmplificator;
    [Range (1,10)]
    [SerializeField] float ZExplosionAmplificator;
    [Range (1,10)]
    [SerializeField] float YExplosionAmplificator;
    [Range (1,10)]
    [SerializeField] float XExplosionAmplificator;
    [SerializeField] float coeffRotate;

    [SerializeField] AnimationCurve ProjectilleBehavior;

    private void Start()
    {
        GetAllParts();

        StartCoroutine(WaitBeforeExplode());
    }

    IEnumerator WaitBeforeExplode()
    {
        yield return new WaitForSeconds(0.5f);
        this.MoveAllParts(SensFx.Explosion);
        yield break;
    }

    void GetAllParts ()
    {
        //Je soustraits de 1 car on assume que l'objet qui sert de ref pour l'explosion est le dernier des enfants de l'objet
        this.allModelParts = new Transform[this.ModelFx.childCount -2];
        this.allPosIni = new Vector3[this.ModelFx.childCount -2];
        this.allRotIni = new Vector3[this.ModelFx.childCount -2];
        this.allDirExplosion = new Vector3[this.ModelFx.childCount -2];
        this.allMatParts = new Material[this.ModelFx.childCount -2];
        
        for (int i = 0; i < this.allModelParts.Length; i++)
        {
            this.allModelParts[i] = this.ModelFx.GetChild(i);
            this.allPosIni[i] = this.allModelParts[i].localPosition;
            this.allRotIni[i] = this.allModelParts[i].localEulerAngles;
            this.allDirExplosion[i] = -(this.RefCenterExplosion.localPosition - this.allPosIni[i]).normalized;
            this.allMatParts[i] = this.allModelParts[i].GetComponent<Renderer>().material;
        }
    }
    
    void MoveAllParts (SensFx sens)
    {
        if (this.allModelParts != null )
        {
            if(sens == SensFx.Explosion)
            {
                for(int i = 0; i < allModelParts.Length; i++)
                {
                    StartCoroutine(BumpOutline(this.allMatParts[i]));
                    Vector3 posToReach = this.allDirExplosion[i] * this.GlobalAmplificator;
                    posToReach.z *= Random.Range(1, this.ZExplosionAmplificator);
                    posToReach.y *= Random.Range(1, this.YExplosionAmplificator);
                    posToReach.x *= Random.Range(1,this.XExplosionAmplificator);
                    Vector3 randomRot = Random.rotation.eulerAngles.normalized;
                    StartCoroutine(MoveFxCoroutine(posToReach, randomRot, this.allModelParts[i], this.allMatParts[i], sens, this.delay));
                }
            }
        }
    }

    IEnumerator MoveFxCoroutine (Vector3 posTarget, Vector3 rotTarget, Transform targetToMove, Material matTarget, SensFx sens, float delay)
    {
        if (delay != 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }
       
        float lerp = 0;
        float newAmount = matTarget.GetFloat(AmountProperty);
        try 
        {
            while (Vector3.Distance(posTarget, targetToMove.localPosition) > 0.05f)
            {    
                lerp = this.ProjectilleBehavior.Evaluate(Time.deltaTime * this.speedAnimation);
                //Partie physics
                targetToMove.localPosition = Vector3.Lerp(targetToMove.localPosition, posTarget, lerp);
                if (sens == SensFx.Explosion)
                {
                    targetToMove.Rotate(rotTarget * this.coeffRotate * lerp);
                    //Partie Dissolve
                    matTarget.SetVector(DirectionProperty, posTarget.normalized);
                    newAmount = Mathf.Lerp(newAmount, this.valMax, lerp);
                    matTarget.SetFloat(AmountProperty, newAmount);
                }
                else
                {
                    targetToMove.Rotate(rotTarget * lerp);
                    /////
                    //Utiliser cette methode si c sur le modèle avec peu de pièces !!
                    //targetToMove.localEulerAngles = Vector3.Lerp(targetToMove.localEulerAngles, rotTarget, lerp);
                    ////
                    //Partie Dissolve
                    matTarget.SetVector(DirectionProperty, posTarget.normalized);
                    newAmount = Mathf.Lerp(newAmount, this.valMin, lerp);
                    matTarget.SetFloat(AmountProperty, newAmount);
                }
                yield return new WaitForSeconds(0);
            }
        }
        finally
        {
            if (sens == SensFx.Integration)
            {
                targetToMove.localEulerAngles = rotTarget;
                targetToMove.localPosition = posTarget;
                matTarget.SetFloat(AmountProperty, valMin);
                StartCoroutine(BumpOutline(matTarget));
            }
        }
        yield break;
    }


    IEnumerator BumpOutline (Material mat)
    {
        this.iniFloatMat = mat.GetFloat(this.OutilneWidthProperty);
        this.iniColorMat = mat.GetColor(this.OutilneColorProperty);

        Color newColor = this.iniColorMat;
        float newWidth = iniFloatMat;
        bool lerpUp = false;
        bool lerpDown = false;
        float lerpColorUp = 0;
        float lerpColorDown = 0;

        try 
        {
            while (lerpUp == false || lerpDown == false)
            {
                if (lerpUp == false)
                {
                    lerpColorUp += this.speedBump;
                    newColor = Color.Lerp(newColor, this.colorBump, lerpColorUp);
                    newWidth = Mathf.Lerp(newWidth, this.widthOutlineBump, lerpColorUp);
                    mat.SetFloat(this.OutilneWidthProperty, newWidth);
                    mat.SetColor(this.OutilneColorProperty, newColor);
                    
                    if (lerpColorUp >= 1)
                    {
                        lerpUp = true;
                    }
                }
                else
                {
                    lerpColorDown += this.speedBump;
                    newColor = Color.Lerp(newColor, this.iniColorMat, lerpColorDown);
                    newWidth = Mathf.Lerp(newWidth, this.iniFloatMat, lerpColorDown);
                    mat.SetFloat(this.OutilneWidthProperty, newWidth);
                    mat.SetColor(this.OutilneColorProperty, newColor);
                    if (lerpColorDown >= 1)
                    {
                        lerpDown = true;
                    }
                }
                yield return new WaitForSeconds(0);
            }
        }
        finally
        {
            mat.SetFloat(this.OutilneWidthProperty, this.iniFloatMat);
            mat.SetColor(this.OutilneColorProperty, this.iniColorMat);
        }
        yield break;
    }
    
}
