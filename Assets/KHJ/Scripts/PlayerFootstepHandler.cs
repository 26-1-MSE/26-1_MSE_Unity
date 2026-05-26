using UnityEngine;

public class PlayerFootstepHandler : MonoBehaviour
{
    public void OnFootstepLeft()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;
        
        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(21);
        else if (sceneIndex == 1)
            AudioManager.SFXInstance?.PlayOneShot(4);
    }

    public void OnFootstepRight()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;
        
        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(20);
        else if (sceneIndex == 1)
            AudioManager.SFXInstance?.PlayOneShot(3);
    }

    public void OnFootstepJump()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;
        
        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(23);
    }
    
    public void OnFootstepGround()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;
        
        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(22);
    }
}
