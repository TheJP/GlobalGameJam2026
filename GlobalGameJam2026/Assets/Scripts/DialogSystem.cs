using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Who
{
    Player, Npc,
}

public enum Dialog
{
    IntroOldRoommate,
    CamVampire,
    CamKawai,
    CamGamer,
    CamCrypto,
    CamNeighbour,
    CamSock,
}

public enum NodeIndex
{
    Greeting,

    PlayerQuestion,
    NpcAnyQuestion,

    NpcAnswer1_1,
    NpcAnswer1_2,
    NpcAnswer2_1,
    NpcAnswer3_1,

    NpcQuestion1,
    NpcQuestion1_Reaction1,
    NpcQuestion1_Reaction2,
    NpcQuestion2,
    NpcQuestion2_Reaction1,
    NpcQuestion2_Reaction2,
    NpcQuestion3,
    NpcQuestion3_Reaction1,
    NpcQuestion3_Reaction2,

    End,
}

public static class DialogSystem
{
    private static PlayerQuestionNode VampireQuestions = new(
        new PlayerQuestion("Ask about his job", "So, you work at a gas station, huh? What’s that like?", NodeIndex.NpcAnswer1_1),
        new PlayerQuestion("Ask about his intentions", "Why do you want to live in a shared flat with me?", NodeIndex.NpcAnswer2_1)
    );

    public static Dictionary<Dialog, DialogTree> Dialogs { get; } = new()
    {
        [Dialog.CamVampire] = new(new()
        {
            [NodeIndex.Greeting] = new SimpleNode(NodeIndex.PlayerQuestion, Who.Npc, "Good day, my dear one. I’d like to introduce myself: My name is Wilfred Novak, but you can call me Willy. I’m excited to make your acquaintance."),

            [NodeIndex.PlayerQuestion] = VampireQuestions,
            [NodeIndex.NpcAnyQuestion] = new NpcAnyQustionNode(NodeIndex.NpcQuestion1, NodeIndex.NpcQuestion2),

            [NodeIndex.NpcAnswer1_1] = new OptionsNode("It’s quiet. I work at night, so I won’t disturb your slumber. But I’d love to be a детский комбайн. Ah, I mean a kindergardener.", new SimpleOption("Do you mean a kindergarten teacher?", NodeIndex.NpcAnswer1_2)),
            [NodeIndex.NpcAnswer1_2] = new SimpleNode(NodeIndex.NpcAnyQuestion, Who.Npc, "Da, of course. Children are delicious , um, delightful."),

            [NodeIndex.NpcAnswer2_1] = new SimpleNode(NodeIndex.NpcAnyQuestion, Who.Npc, "I'm looking for new comrades. Life in my residence has been rather dull. I seek for new connections to share refreshments with. Or whatever it is people do these days."),

            [NodeIndex.NpcQuestion1] = new OptionsNode("I appreciate the company of fellow humans. Do mortals frequently visit this habitat?",
                new SimpleOption("Yes, my friends and I love to hang out in my living room.", NodeIndex.NpcQuestion1_Reaction1),
                new SimpleOption("No, I’m a lone wolf.", NodeIndex.NpcQuestion1_Reaction2)
            ),
            [NodeIndex.NpcQuestion1_Reaction1] = new SimpleNode(NodeIndex.PlayerQuestion, Who.Npc, "Wonderful..."),
            [NodeIndex.NpcQuestion1_Reaction2] = new SimpleNode(NodeIndex.PlayerQuestion, Who.Npc, " Oh… That’s too bad."),

            [NodeIndex.NpcQuestion2] = new OptionsNode("How is the parking situation? I wish to bring my automobiles with me.",
                new SimpleOption("There are a lot of parking spaces. I’ll ring the landlord to set you up with some.", NodeIndex.NpcQuestion2_Reaction1),
                new SimpleOption("Cars? Plural? Good luck with that, I can’t even get a space for my own ride.", NodeIndex.NpcQuestion2_Reaction2)
            ),
            [NodeIndex.NpcQuestion2_Reaction1] = new SimpleNode(NodeIndex.PlayerQuestion, Who.Npc, "You’re too kind my cherished friend."),
            [NodeIndex.NpcQuestion2_Reaction2] = new SimpleNode(NodeIndex.PlayerQuestion, Who.Npc, "That’s unfortunate."),
        }),
    };
}

public class DialogTree
{
    public Dictionary<NodeIndex, IDialogNode> Nodes { get; private set; }
    public string InterruptMessage { get; set; }

    public DialogTree(Dictionary<NodeIndex, IDialogNode> nodes, string interruptMessage = null)
    {
        Nodes = nodes;
        InterruptMessage = interruptMessage;
    }
}

public interface IDialogNode
{
    void Enter(DialogStateMachine stateMachine);
    void OptionSelected(DialogStateMachine stateMachine, int selectedOption) { }
}

public class SimpleNode : IDialogNode
{
    public Who Who { get; }
    public string Prompt { get; }
    public NodeIndex NextNode { get; }

    public SimpleNode(NodeIndex nextNode, Who who, string prompt)
    {
        Who = who;
        Prompt = prompt;
        NextNode = nextNode;
    }

    public void Enter(DialogStateMachine stateMachine)
    {
        stateMachine.ShowDialog(Who, Prompt);
        stateMachine.Transition(NextNode);
    }
}

public class OptionsNode : IDialogNode
{
    public string Prompt { get; private set; }
    public SimpleOption[] Options { get; private set; }
    public IList<string> Answers => Options.Select(o => o.Text).ToList();

    public void Enter(DialogStateMachine stateMachine)
    {
        stateMachine.ShowDialog(Who.Npc, Prompt);
        stateMachine.ShowOptions(Answers);
    }

    public void OptionSelected(DialogStateMachine stateMachine, int selectedOption) => stateMachine.Transition(Options[selectedOption].NextNode);

    public OptionsNode(string prompt, params SimpleOption[] options)
    {
        Prompt = prompt;
        Options = options;
    }
}

public record SimpleOption
{
    public string Text { get; }
    public NodeIndex NextNode { get; }

    public SimpleOption(string answer, NodeIndex nextNode)
    {
        Text = answer;
        NextNode = nextNode;
    }
}

public class NpcAnyQustionNode : IDialogNode
{
    private IList<NodeIndex> PossibleQuestions { get; }

    public NpcAnyQustionNode(params NodeIndex[] possibleQuestions)
    {
        PossibleQuestions = possibleQuestions;
    }

    public void Enter(DialogStateMachine stateMachine)
    {
        if (!stateMachine.Data.ContainsKey(DataIndex.NpcAskedQuestions))
        {
            stateMachine.Data.Add(DataIndex.NpcAskedQuestions, new HashSet<NodeIndex>());

        }
        var askedQuestions = stateMachine.Data[DataIndex.NpcAskedQuestions] as ISet<NodeIndex>;

        var openQuestions = PossibleQuestions.Where(q => !askedQuestions.Contains(q)).ToList();
        if (openQuestions.Count == 0)
        {
            stateMachine.Transition(NodeIndex.End);
        }
        else
        {
            var node = openQuestions[Random.Range(0, openQuestions.Count)];
            askedQuestions.Add(node);
            stateMachine.Transition(node);
        }
    }
}

public class PlayerQuestionNode : IDialogNode
{
    public PlayerQuestion[] PossibleOptions { get; }
    private IList<PlayerQuestion> LastShownOptions { get; set; }

    public PlayerQuestionNode(params PlayerQuestion[] possibleOptions)
    {
        PossibleOptions = possibleOptions;
    }

    public void Enter(DialogStateMachine stateMachine)
    {
        if (!stateMachine.Data.ContainsKey(DataIndex.PlayerAskedQuestions))
        {
            stateMachine.Data.Add(DataIndex.PlayerAskedQuestions, new HashSet<PlayerQuestion>());
        }
        var askedQuestions = stateMachine.Data[DataIndex.PlayerAskedQuestions] as ISet<PlayerQuestion>;

        var openQuestions = PossibleOptions.Where(q => !askedQuestions.Contains(q)).ToList();
        if (openQuestions.Count == 0)
        {
            stateMachine.Transition(NodeIndex.End);
        }
        else
        {
            LastShownOptions = openQuestions;
            stateMachine.ShowOptions(openQuestions.Select(o => o.Option).ToList());
        }
    }

    public void OptionSelected(DialogStateMachine stateMachine, int selectedOption)
    {
        var option = LastShownOptions[selectedOption];
        stateMachine.ShowDialog(Who.Player, option.Text);
        stateMachine.Transition(option.NextNode);
    }
}

public class PlayerQuestion
{
    public string Option { get; }
    public string Text { get; }
    public NodeIndex NextNode { get; }

    public PlayerQuestion(string option, string text, NodeIndex nextNode)
    {
        Option = option;
        Text = text;
        NextNode = nextNode;
    }
}

public enum DataIndex
{
    NpcAskedQuestions,
    PlayerAskedQuestions,
}

public class DialogStateMachine
{
    public event System.Action<Who, string> OnShowDialog;
    public event System.Action<IList<string>> OnShowOptions;

    public DialogTree DialogTree { get; private set; }
    public NodeIndex Node { get; set; }

    /// <summary>Data that lives for the duration of the dialog.</summary>
    public Dictionary<DataIndex, object> Data { get; } = new();

    public IDialogNode DialogNode => DialogTree.Nodes[Node];

    public void StartDialog(Dialog dialog, NodeIndex startNode = 0) => StartDialog(DialogSystem.Dialogs[dialog], startNode);

    public void StartDialog(DialogTree dialogTree, NodeIndex startNode = 0)
    {
        DialogTree = dialogTree;
        Node = startNode;
        Data.Clear();
        Transition(startNode);
    }

    public void ShowDialog(Who who, string message) => OnShowDialog?.Invoke(who, message);
    public void ShowOptions(IList<string> options) => OnShowOptions?.Invoke(options);
    public void Transition(NodeIndex nextNode)
    {
        Node = nextNode;
        DialogNode.Enter(this);
    }
}
