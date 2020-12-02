using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FirstChallenge : MonoBehaviour
{
    [SerializeField] private TextAsset values;
    [SerializeField] private Transform prefab;
    [SerializeField] private Transform plus;
    [SerializeField] private TextMeshPro equals;
    [SerializeField, ColorUsage(true, true)] private Color correctColor;
    
    private IEnumerator Start()
    {
        // grab every number as a string
        var stringVals = values.text.Split('\n');
        // parse the strings with linq because I'm lazy
        var intVals = stringVals.Select(int.Parse).ToList();

        // init a list for the transforms 
        var objects = new List<Transform>(stringVals.Length);
        // init a list for the linerenderers so I don't have to GetComponent on every o(n^2) operation because I'm not THAT lazy 
        var lines = new List<LineRenderer>(stringVals.Length);
        
        // loop through the strings and instantiate the objects
        foreach (var s in stringVals)
        {
            // split the string down the middle to get two numbers (this won't work if the number is a single digit!!) 
            var takeAmount = Mathf.FloorToInt(s.Length / 2f);
            // the first number is incredibly likely to be in the 10-20 range
            // this means all of these values will be very similar
            // similar values = poor distribution. Better to have them at a similar radius rather than a similar angle round the circle!
            var radius = int.Parse(s.Substring(0, takeAmount));
            // the second number will be completely random. Use this as the angle around the circle for better distribution
            var angle = int.Parse(s.Substring(takeAmount));

            // use the numbers to position the new object
            // multiply by .2 so everything fits in the camera view
            var pos = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)) * .2f;
            
            // create the new object
            var newObject = Instantiate(prefab, pos, Quaternion.identity);
            // add it to the list
            objects.Add(newObject);
            // set the text for the big reveal at the end (dirty hack, this means the text's font size is tiny)
            newObject.GetComponentInChildren<TextMeshPro>().text = s;
            // add the child line renderer to the list
            lines.Add(newObject.GetComponentInChildren<LineRenderer>());
        }
        
        // initialised some variables for the correct values
        int correctI = -1, correctJ = -1;
        SpriteRenderer correctObjI = null, correctObjJ = null;
        
        // grab a timestamp so we can see how long it tool
        var time = DateTime.Now;
        
        // loop through the items twice to compare them against each other
        for (int i = 0; i < intVals.Count; i++)
        {
            // store the current object and int value
            var objI = objects[i];
            var intI = intVals[i];
            
            for (int j = 1; j < intVals.Count - 1; j++)
            {
                // check the next value in the list because it looks pretty
                // (and executes quicker but I only know that because I did the task properly first...) 
                var next = i + j;

                // j = 1
                // i = 197
                // i + j = 200
                // we need 0

                // j = 3
                // i = 197
                // count - i - j = 0

                // j = 3
                // i = 198
                // i + j = 201
                // we want 1
                // count - i - j = -1

                // j = 3
                // i = 199
                // i + j = 202

                // keep the "next" value in range
                // there's probably a better way to do this
                // but the whole thing is terrible so it doesn't matter anyway
                if (next >= intVals.Count)
                    next = -(intVals.Count - i - j);
                

                // grab references to the current object, int value and line renderer
                var objJ = objects[next];
                var intJ = intVals[next];
                var lineJ = lines[next];

                // set the positions of the line
                lineJ.SetPositions(new[] {objI.position, objJ.position});

                // check if it's correct
                var isCorrect = (intI + intJ) == 2020;
                
                if (isCorrect)
                {
                    // store the values
                    correctI = intI;
                    correctJ = intJ;
                    correctObjI = objI.GetComponentInChildren<SpriteRenderer>();
                    correctObjJ = objJ.GetComponentInChildren<SpriteRenderer>();

                    // break out of the loop
                    break;
                }
                
                // if the current "j" index is divisible by 10, wait a frame
                // basically, do 10 checks per frame
                if (j % 10 == 0)
                    yield return null;
            }

            // if we've got a correct answer, break out of the loop
            if (correctI != -1 && correctJ != -1)
                break;
        }

        // grab a pair of colours for fading the lines
        // white with no alpha
        var clearCol = new Color2(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0));

        // fade the lines with DOTween
        foreach (var line in lines)
            line.DOColor(new Color2(line.startColor, line.endColor), clearCol, 0.75f);
        

        // if there are no correct objects, break out of the loop
        // this will never happen, but it's best to be sure
        if (correctObjI == null || correctObjJ == null)
            yield break;

        // fade the correct circles to green
        correctObjI.DOColor(correctColor, 0.75f);
        correctObjJ.DOColor(correctColor, 0.75f);

        // scale them up so we can see the numbers inside
        var parentI = correctObjI.transform.parent;
        parentI.DOScale(Vector3.one * 20, 0.75f).SetEase(Ease.InOutQuint);
        var parentJ = correctObjJ.transform.parent;
        parentJ.DOScale(Vector3.one * 20, 0.75f).SetEase(Ease.InOutQuint);
        
        // move them into position 
        parentI.DOMove(new Vector3(-4, 2, 0), 0.75f).SetEase(Ease.InOutQuint);
        parentJ.DOMove(new Vector3(4, 2, 0), 0.75f).SetEase(Ease.InOutQuint);
        
        // remove them from the list 
        objects.Remove(parentI);
        objects.Remove(parentJ);
        
        // loop through what's left and scale them down with DOTween
        foreach (var obj in objects)
            obj.DOScale(Vector3.zero, 0.75f).SetEase(Ease.InOutQuint);
        
        // scale up the plus sign with DOTween
        plus.DOScale(Vector3.one, 0.75f).SetEase(Ease.InOutQuint);
        // scale up the answer with DOTween
        equals.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.InOutQuint);
        // set the answer text to the correct value
        equals.text = $"= {correctI * correctJ}";
        
        // log out all of the info we found, including time taken
        Debug.Log($"Found answer of {correctI * correctJ} from {correctI} and {correctJ} in {(DateTime.Now - time).TotalSeconds}s");
    }
}
