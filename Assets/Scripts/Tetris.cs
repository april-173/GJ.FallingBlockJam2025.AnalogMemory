using UnityEngine;
using UnityEngine.InputSystem;

public class Tetris : MonoBehaviour
{
    public AudioClip tetrisSound;

    private float tetrisTime = 3;
    private float tetrisTimer = 0;
    private bool tetris_Tetris = false;
    private bool tetris_tEtris = false;
    private bool tetris_teTris = false;
    private bool tetris_tetRis = false;
    private bool tetris_tetrIs = false;
    private bool tetris_tetriS = false;

    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (tetrisTimer > 0) tetrisTimer -= Time.deltaTime;

        if (keyboard[Key.T].wasPressedThisFrame && tetrisTimer <= 0) 
        {
            tetrisTimer = tetrisTime;
            tetris_Tetris = true;
        }

        if (keyboard[Key.E].wasPressedThisFrame && tetris_Tetris)
        {
            tetrisTimer = tetrisTime;
            tetris_tEtris= true;
        }

        if (keyboard[Key.T].wasPressedThisFrame && tetris_tEtris)
        {
            tetrisTimer = tetrisTime;
            tetris_teTris= true;
        }

        if (keyboard[Key.R].wasPressedThisFrame && tetris_teTris)
        {
            tetrisTimer = tetrisTime;
            tetris_tetRis= true;
        }

        if (keyboard[Key.I].wasPressedThisFrame && tetris_tetRis)
        {
            tetrisTimer = tetrisTime;
            tetris_tetrIs= true;
        }

        if (keyboard[Key.S].wasPressedThisFrame && tetris_tetrIs)
        {
            tetrisTimer = tetrisTime;
            tetris_tetriS= true;
        }

        if(tetrisTimer <= 0)
        {
            tetrisTimer = 0;
            tetris_Tetris= false;
            tetris_tEtris = false;
            tetris_teTris = false;
            tetris_tetRis = false;
            tetris_tetrIs= false;
            tetris_tetriS = false;
        }

        if(tetris_tetriS)
        {
            GetComponent<AudioSource>().PlayOneShot(tetrisSound);
            tetrisTimer = 0;
        }
    }
}
