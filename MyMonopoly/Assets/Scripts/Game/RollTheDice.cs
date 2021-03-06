using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RollTheDice : MonoBehaviour
{

    [SerializeField] Texture[] diceSides = new Texture[6];
    RawImage rend;

    public int finalSide = 0;
    public bool isRolling = true;

    void Awake()
    {
        rend = GetComponent<RawImage>();
    }

/*
    public int Roll()
    {
        StartCoroutine(IERollTheDice());
        return finalSide;
    }

    IEnumerator IERollTheDice()
    {
        int randomDiceSide = 0;

        for (int i = 0; i <= 20; i++) {
            randomDiceSide = Random.Range(0, 5);
            rend.texture = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        finalSide = randomDiceSide + 1;
    }

    public int GetLastRollResult()
    {
        return finalSide;
    }
    */

    public void Roll(int finalSide)
    {
        isRolling = true;
        this.finalSide = finalSide;
        StartCoroutine(IERollTheDice());
    }

    IEnumerator IERollTheDice()
    {
        int randomDiceSide;

        for (int i = 0; i <= 10; i++) {
            randomDiceSide = Random.Range(0, 5);
            rend.texture = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.1f);
        }

        rend.texture = diceSides[finalSide - 1];
        isRolling = false;
    }

}
