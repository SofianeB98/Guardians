using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InputName
{
    public static string Horizontal = "Horizontal";
    public static string Vertical = "Vertical";
    public static string Jump = "Jump";
    public static string MouseHorizontal = "Mouse X";
    public static string MouseVertical = "Mouse Y";

    public static string Bucheronner = "Bucheronner";
    public static string LancerDeHache = "LaunchAxe";

    public static string Sprint = "Sprint";
    public static string CameraLibre = "CameraLibre";

    public static string ResetCurve = "ResetCurve";
    public static string CancelLaunch = "CancelLaunch";

    public static string ChangeSeedSelection = "ChangeSeedSelection";
    public static string SeedLaunch = "SeedLaunch";

    public static void Init(string extenstion)
    {
        if (extenstion != "?")
        {
            Horizontal = "Horizontal" + extenstion;
            Vertical = "Vertical" + extenstion;
            Jump = "Jump" + extenstion;
            MouseHorizontal = "Mouse X" + extenstion;
            MouseVertical = "Mouse Y" + extenstion;
            Bucheronner = "Bucheronner" + extenstion;
            LancerDeHache = "LaunchAxe" + extenstion;
            Sprint = "Sprint" + extenstion;
            CameraLibre = "CameraLibre" + extenstion;
            ResetCurve = "ResetCurve" + extenstion;
            CancelLaunch = "CancelLaunch" + extenstion;
            ChangeSeedSelection = "ChangeSeedSelection" + extenstion;
            SeedLaunch = "SeedLaunch" + extenstion;
        }
        else
        {
            Horizontal = "Horizontal";
            Vertical = "Vertical";
            Jump = "Jump";
            MouseHorizontal = "Mouse X";
            MouseVertical = "Mouse Y";
            Bucheronner = "Bucheronner";
            LancerDeHache = "LaunchAxe";
            Sprint = "Sprint";
            CameraLibre = "CameraLibre";
            ResetCurve = "ResetCurve";
            CancelLaunch = "CancelLaunch";
            ChangeSeedSelection = "ChangeSeedSelection";
            SeedLaunch = "SeedLaunch";
        }
        
    }
}
