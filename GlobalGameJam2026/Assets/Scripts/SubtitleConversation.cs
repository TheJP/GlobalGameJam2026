using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SubtitleConversation : MonoBehaviour
{
    [field: SerializeField]
    public Dialog Dialog { get; set; }

    [field: SerializeField]
    public Font PlayerFont { get; set; }

    [field: SerializeField]
    public Font NpcFont { get; set; }

    [field: SerializeField]
    public Color PlayerColor { get; set; }

    [field: SerializeField]
    public Color NpcColor { get; set; }

    private UIDocument document;
    private VisualElement root;
    private VisualElement subtitlesParent;

    private readonly Queue<ICommand> commands = new();

    public IEnumerator Start()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        subtitlesParent = root.Q("Subtitles");
        subtitlesParent.Clear();

        var vampireStateMachine = new DialogStateMachine();
        vampireStateMachine.OnShowDialog += (who, text) => commands.Enqueue(new TextCommand(who, text));
        vampireStateMachine.OnShowOptions += (options) => commands.Enqueue(new OptionsCommand(options));
        vampireStateMachine.StartDialog(Dialog.CamVampire);


        int? selectedOption = null;
        ICommand currentCommand = null;

        while (true)
        {
            yield return new WaitForSeconds(0.15f);

            if (currentCommand == null)
            {
                if (commands.Count == 0)
                {
                    continue;
                }
                else
                {
                    currentCommand = commands.Dequeue();
                    if (currentCommand is TextCommand)
                    {
                        subtitlesParent.Clear();
                    }
                }
            }

            if (currentCommand is TextCommand textCommand)
            {
                if (textCommand.Text.Count == 0)
                {
                    currentCommand = null;
                    //var lineBreak = new VisualElement();
                    //lineBreak.style.width = new(new Length(100f, LengthUnit.Percent));
                    //subtitlesParent.Add(lineBreak);
                    continue;
                }

                var word = textCommand.Text.Dequeue();
                var label = new Label(word);
                label.style.marginLeft = new(20f);
                label.style.opacity = new(0f);
                if (textCommand.Who == Who.Npc)
                {
                    label.style.color = new(NpcColor);
                    label.style.unityFont = NpcFont;
                }
                else if (textCommand.Who == Who.Player)
                {
                    label.style.color = new(PlayerColor);
                    label.style.unityFont = PlayerFont;
                }
                subtitlesParent.Add(label);

                yield return null;
                label.style.marginLeft = new(0f);
                label.style.opacity = new(1f);

                var trimmedWord = word.Trim();
                if (trimmedWord.EndsWith('.') || trimmedWord.EndsWith('!') || trimmedWord.EndsWith('?') || trimmedWord.EndsWith(':') || trimmedWord.EndsWith(';'))
                {
                    yield return new WaitForSeconds(0.5f);
                }

            }
            else if (currentCommand is OptionsCommand optionsCommand)
            {
                // TODO
                Debug.Log(optionsCommand.Options);
            }

            //string word = "";
            //for (int i = Random.Range(3, 10); i >= 0; --i)
            //{
            //    word += (char)Random.Range((int)'a', (int)'z' + 1);
            //}
        }
    }
}

interface ICommand { }

class TextCommand : ICommand
{
    public Who Who { get; set; }
    public Queue<string> Text { get; set; }

    public TextCommand(Who who, string text)
    {
        Who = who;
        Text = new(text.Split());
    }
}

class OptionsCommand : ICommand
{
    public IList<string> Options { get; set; }

    public OptionsCommand(IList<string> options) => Options = options;
}