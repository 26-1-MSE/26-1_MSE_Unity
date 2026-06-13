using UnityEngine;

/// <summary>
/// Plays footstep and jump/landing sound effects based on the current scene,
/// triggered via animation events on the player.
/// </summary>
public class PlayerFootstepHandler : MonoBehaviour
{
    /// <summary>
    /// Called by an animation event when the left foot touches the ground.
    /// </summary>
    public void OnFootstepLeft()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;

        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(21);
        else if (sceneIndex == 1)
            AudioManager.SFXInstance?.PlayOneShot(4);
    }

    /// <summary>
    /// Called by an animation event when the right foot touches the ground.
    /// </summary>
    public void OnFootstepRight()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;

        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(20);
        else if (sceneIndex == 1)
            AudioManager.SFXInstance?.PlayOneShot(3);
    }

    /// <summary>
    /// Called by an animation event when the player jumps.
    /// </summary>
    public void OnFootstepJump()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;

        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(23);
    }

    /// <summary>
    /// Called by an animation event when the player lands on the ground.
    /// </summary>
    public void OnFootstepGround()
    {
        int sceneIndex = GameManager.Instance?.CurrentSceneIndex ?? -1;

        if (sceneIndex == 3)
            AudioManager.SFXInstance?.PlayOneShot(22);
    }
}
