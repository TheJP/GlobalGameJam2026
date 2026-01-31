using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SubtitleConversation : MonoBehaviour
{
    UIDocument document;
    VisualElement root;
    VisualElement subtitlesParent;

    public IEnumerator Start()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        subtitlesParent = root.Q("Subtitles");
        subtitlesParent.Clear();

        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            string word = "";
            for (int i = Random.Range(3, 10); i >= 0; --i)
            {
                word += (char)Random.Range((int)'a', (int)'z' + 1);
            }

            var label = new Label(word);
            label.style.marginLeft = new(20f);
            label.style.opacity = new(0f);
            subtitlesParent.Add(label);

            yield return null;
            label.style.marginLeft = new(0f);
            label.style.opacity = new(1f);
        }
    }
}
