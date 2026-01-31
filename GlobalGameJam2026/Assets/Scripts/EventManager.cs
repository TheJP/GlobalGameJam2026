using UnityEngine;

static class EventManager
{
    public delegate void GenericDelegate();
    
    public static GenericDelegate BackToStartEvent;
    
    //Intro
    public static GenericDelegate ProgressToIntroEvent;
    
    //Letters
    public static GenericDelegate ProgressToLettersEvent;
    
    //Zoom call
    public static GenericDelegate ProgressToZoomCallEvent;
    
    //Sofa
    public static GenericDelegate ProgressToEndEvent;
}
