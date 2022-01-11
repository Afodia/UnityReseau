using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RollTheDice : MonoBehaviour
{

    [SerializeField] Texture[] diceSides = new Texture[6];
    RawImage rend;

    int finalSide = 0;

    void Awake()
    {
        rend = GetComponent<RawImage>();
    }

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
}
