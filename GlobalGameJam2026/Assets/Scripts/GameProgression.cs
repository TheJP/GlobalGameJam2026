using System;
using UnityEngine;

public class GameProgression : MonoBehaviour
{
    [SerializeField] private GameObject SequenceStart, SequenceIntro, SequenceLetters, SequenceZoomCall, SequenceEnd;

    private enum GameProgressionState
    {
        start,
        intro,
        letters,
        zoomCall,
        end,
    }
    
    private GameProgressionState currentState = GameProgressionState.start;

    //Trigger Events
    public void RestartGame()
    {
        if (currentState == GameProgressionState.start) return;
        currentState = GameProgressionState.start;
        
        ActivateSequence(SequenceStart);
    }
    public void ProgressToIntro()
    {
        if (currentState == GameProgressionState.intro) return;
        currentState = GameProgressionState.intro;
        
        ActivateSequence(SequenceIntro);
    }
    public void ProgressToLetters()
    {
        if (currentState == GameProgressionState.letters) return;
        currentState = GameProgressionState.letters;
        
        ActivateSequence(SequenceLetters);

    }
    public void ProgressToZoomCall()
    {
        if (currentState == GameProgressionState.zoomCall) return;
        currentState = GameProgressionState.zoomCall;
        
        ActivateSequence(SequenceZoomCall);
    }
    public void ProgressToEnd()
    {
        if (currentState == GameProgressionState.end) return;
        currentState = GameProgressionState.end;
        
        ActivateSequence(SequenceEnd);
    }

    private void ActivateSequence(GameObject sequence)
    {
        SequenceStart.SetActive(false); 
        SequenceIntro.SetActive(false);
        SequenceLetters.SetActive(false);
        SequenceZoomCall.SetActive(false);
        SequenceEnd.SetActive(false);
        
        sequence.SetActive(true);
    }
}
