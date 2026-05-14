namespace Net.MyStuff.MyTool;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Forms;

// player data class for state observations 
public class PlayerData
{
    public long p1Health {  get; set; }
    public long p2Health { get; set; }
    public long p1ButtonHitBox { get; set; }
    public long p2ButtonHitBox { get; set; }
    public long p1Action { get; set; }
    public long p2Action { get; set; }
    public long p1MoveDirection {  get; set; }
    public long p2MoveDirection { get; set; }
    public long p1MovementState { get; set; }
    public long p2MovementState { get; set; }
    public long p1InAir { get; set; }
    public long p2InAir { get; set; }
    public long p1AttackInfo { get; set; }
    public long p2AttackInfo { get; set; }
    public long p1IsCrouching { get; set; }
    public long p2IsCrouching { get; set; }
    public long p1IsAttacking { get; set; }
    public long p2IsAttacking { get; set; }
    public long p1DistanceFromEnemy { get; set; }
    public long p2DistanceFromEnemy { get; set; }
    public long p1FacingLeft { get; set; }
    public long p2FacingLeft { get; set; }
    public long p1FireballPosition { get; set; }
    public long p2FireballPosition { get; set; }
    public long p1XPos { get; set; }
    public long p2XPos { get; set; }
    public long p1YPos { get; set; }
    public long p2YPos { get; set; }
    public long p1YVelocity { get; set; }
    public long p2YVelocity { get; set; }
    public long p1RoundWins { get; set; }
    public long p2RoundWins { get; set; }

    public PlayerData(
       long p1Health,
       long p2Health,
       long p1ButtonHitBox,
       long p2ButtonHitBox,
       long p1Action,
       long p2Action,
       long p1MoveDirection,
       long p2MoveDirection,
       long p1MovementState,
       long p2MovementState,
       long p1InAir,
       long p2InAir,
       long p1AttackInfo,
       long p2AttackInfo,
       long p1IsCrouching,
       long p2IsCrouching,
       long p1IsAttacking,
       long p2IsAttacking,
       long p1DistanceFromEnemy,
       long p2DistanceFromEnemy,
       long p1FacingLeft,
       long p2FacingLeft,
       long p1FireballPosition,
       long p2FireballPosition,
       long p1XPos,
       long p2XPos,
       long p1YPos,
       long p2YPos,
       long p1YVelocity,
       long p2YVelocity,
       long p1RoundWins,
       long p2RoundWins
    )
    {
        this.p1Health = p1Health;
        this.p2Health = p2Health;
        this.p1ButtonHitBox = p1ButtonHitBox;
        this.p2ButtonHitBox = p2ButtonHitBox;
        this.p1Action = p1Action;
        this.p2Action = p2Action;
        this.p1MoveDirection = p1MoveDirection;
        this.p2MoveDirection = p2MoveDirection;
        this.p1MovementState = p1MovementState;
        this.p2MovementState = p2MovementState;
        this.p1InAir = p1InAir;
        this.p2InAir = p2InAir;
        this.p1AttackInfo = p1AttackInfo;
        this.p2AttackInfo = p2AttackInfo;
        this.p1IsCrouching = p1IsCrouching;
        this.p2IsCrouching = p2IsCrouching;
        this.p1IsAttacking = p1IsAttacking;
        this.p2IsAttacking = p2IsAttacking;
        this.p1DistanceFromEnemy = p1DistanceFromEnemy;
        this.p2DistanceFromEnemy = p2DistanceFromEnemy;
        this.p1FacingLeft = p1FacingLeft;
        this.p2FacingLeft = p2FacingLeft;
        this.p1FireballPosition = p1FireballPosition;
        this.p2FireballPosition = p2FireballPosition;
        this.p1XPos = p1XPos;
        this.p2XPos = p2XPos;
        this.p1YPos = p1YPos;
        this.p2YPos = p2YPos;
        this.p1YVelocity = p1YVelocity;
        this.p2YVelocity = p2YVelocity;
        this.p1RoundWins = p1RoundWins;
        this.p2RoundWins = p2RoundWins;
    }
}

[ExternalTool("SFAutomation")]
public sealed class SFAutomationForm : ToolFormBase, IExternalToolForm
{
    // relevant memory addresses for observations 
    private long p1HPAddr = 0x000530;
    private long p2HPAddr = 0x000730;
    private long p1Action = 0x000503;
    private long p2Action = 0x000703;
    private long p1MoveDirection = 0x00053A;
    private long p2MoveDirection = 0x00073A;
    private long p1HitboxActive = 0x00053E;
    private long p2HitboxActive = 0x00073E;
    private long p1MovementState = 0x00053F;
    private long p2MovementState = 0x00073F;
    private long p1InAir = 0x000540;
    private long p2InAir = 0x000740;
    private long p1AttackInfo = 0x000541;
    private long p2AttackInfo = 0x000741;
    private long p1IsCrouching = 0x000544;
    private long p2IsCrouching = 0x000744;
    private long p1IsAttacking = 0x0005E9;
    private long p2IsAttacking = 0x0007E9;
    private long p1DistanceFromEnemy = 0x0005EB;
    private long p2DistanceFromEnemy = 0x0007EB;
    private long p1FacingLeft = 0x0005F3;
    private long p2FacingLeft = 0x0007F3;
    private long p1FireballPosition = 0x000907;
    private long p2FireballPosition = 0x000957;
    private long p1XPos = 0x000507;
    private long p2XPos = 0x000707;
    private long p1YPos = 0x00050A;
    private long p2YPos = 0x00070A;
    private long p1YVelocity = 0x00052F;
    private long p2YVelocity = 0x00072F;
    private long p1RoundWins = 0x0005D0;
    private long p2RoundWins = 0x0007D0;

    private long roundTimerAddr = 0x0018F3;
    private long? roundTimer = null;

    private PlayerData? playerData;
    private Dictionary<string, bool>? inputDict;
    private int framesElapsed = 0;
    private Socket.SocketServer socketServer = new();
    private bool reseting = false;

    // possible inputs for SNES controller
    private enum INPUTS
    {
        forward,
        backward,
        up,
        down,
        upforward,
        upbackward,
        downforward,
        downbackward,
        Y,
        B,
        X,
        A,
        L,
        R
    }
    private Queue<string> moveQueue = new();
    private int executionFrames = 0;
    private bool airActionAvailable = true;
    private bool player2 = false;
    private string[] lshoryuken = ["FORWARD", "DOWN", "DOWNFORWARD+Y"];
    private string[] lhadouken = ["DOWN", "DOWNFORWARD", "FORWARD+Y"];
    private string[] ltatsu = ["DOWN", "DOWNBACKWARD", "BACKWARD+B"];
    private string[] mshoryuken = ["FORWARD", "DOWN", "DOWNFORWARD+X"];
    private string[] mhadouken = ["DOWN", "DOWNFORWARD", "FORWARD+X"];
    private string[] mtatsu = ["DOWN", "DOWNBACKWARD", "BACKWARD+A"];
    private string[] hshoryuken = ["FORWARD", "DOWN", "DOWNFORWARD+L"];
    private string[] hhadouken = ["DOWN", "DOWNFORWARD", "FORWARD+L"];
    private string[] htatsu = ["DOWN", "DOWNBACKWARD", "BACKWARD+R"];

    protected override string WindowTitleStatic // required when superclass is ToolFormBase or FormBase
        => "SFAutomation";
   
    public ApiContainer? _maybeAPIContainer { get; set; }

    private ApiContainer APIs
        => _maybeAPIContainer!;

    public override async void Restart()
    {
        reseting = true;
        if (socketServer.GetIsRunning()) return;

        try
        {
            await socketServer.StartServer();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    protected override async void UpdateBefore()
    {
        long p1Health = ReadMemory(this.p1HPAddr);
        long p2Health = ReadMemory(this.p2HPAddr);
        long p1ButtonHitBox = ReadMemory(this.p1HitboxActive);
        long p2ButtonHitBox = ReadMemory(this.p2HitboxActive);
        long p1Action = ReadMemory(this.p1Action);
        long p2Action = ReadMemory(this.p2Action);
        long p1MoveDirection = ReadMemory(this.p1MoveDirection);
        long p2MoveDirection = ReadMemory(this.p2MoveDirection);
        long p1MovementState = ReadMemory(this.p1MovementState);
        long p2MovementState = ReadMemory(this.p2MovementState);
        long p1InAir = ReadMemory(this.p1InAir);
        long p2InAir = ReadMemory(this.p2InAir);
        long p1AttackInfo = ReadMemory(this.p1AttackInfo);
        long p2AttackInfo = ReadMemory(this.p2AttackInfo);
        long p1IsCrouching = ReadMemory(this.p1IsCrouching);
        long p2IsCrouching = ReadMemory(this.p2IsCrouching);
        long p1IsAttacking = ReadMemory(this.p1IsAttacking);
        long p2IsAttacking = ReadMemory(this.p2IsAttacking);
        long p1DistanceFromEnemy = ReadMemory(this.p1DistanceFromEnemy);
        long p2DistanceFromEnemy = ReadMemory(this.p2DistanceFromEnemy);
        long p1FacingLeft = ReadMemory(this.p1FacingLeft);
        long p2FacingLeft = ReadMemory(this.p2FacingLeft);
        long p1FireballPosition = ReadMemory(this.p1FireballPosition);
        long p2FireballPosition = ReadMemory(this.p2FireballPosition);
        long p1XPos = ReadMemory(this.p1XPos);
        long p2XPos = ReadMemory(this.p2XPos);
        long p1YPos = ReadMemory(this.p1YPos);
        long p2YPos = ReadMemory(this.p2YPos);
        long p1YVelocity = ReadMemory(this.p1YVelocity);
        long p2YVelocity = ReadMemory(this.p2YVelocity);
        long p1RoundWins = ReadMemory(this.p1RoundWins);
        long p2RoundWins = ReadMemory(this.p2RoundWins);

        this.roundTimer = ReadMemory(this.roundTimerAddr);


        // press start button ever 60 frames while in between episodes
        if (this.roundTimer == 0)
        {
            if (this.player2 == false && framesElapsed % 60 == 0)
            {
                inputDict = InitInputDict();
                inputDict["P1 Start"] = true;
                
                APIs.Joypad.Set(inputDict);
            } else if (this.player2 == true && framesElapsed % 300 == 0)
            {
                inputDict = InitInputDict();
                inputDict["P2 Start"] = true;
                
                APIs.Joypad.Set(inputDict);
            }
        }
        else
        {
            reseting = false;
        }

        playerData = new PlayerData(
            p1Health,
            p2Health,
            p1ButtonHitBox,
            p2ButtonHitBox,
            p1Action,
            p2Action,
            p1MoveDirection,
            p2MoveDirection,
            p1MovementState,
            p2MovementState,
            p1InAir,
            p2InAir,
            p1AttackInfo,
            p2AttackInfo,
            p1IsCrouching,
            p2IsCrouching,
            p1IsAttacking,
            p2IsAttacking,
            p1DistanceFromEnemy,
            p2DistanceFromEnemy,
            p1FacingLeft,
            p2FacingLeft,
            p1FireballPosition,
            p2FireballPosition,
            p1XPos,
            p2XPos,
            p1YPos,
            p2YPos,
            p1YVelocity,
            p2YVelocity,
            p1RoundWins,
            p2RoundWins
         );

        string jsonPlayerData = JsonSerializer.Serialize(playerData);
        socketServer.SendToClient(jsonPlayerData);


        var action = "None";

        if (!reseting)
        {
            action = socketServer.GetActionToPerform();
        }
            
        if (action == "player2")
        {
            this.player2 = true;
        }

        // initialize reset of game state (soft reset)
        if (action == "reset" && player2 == false)
        {
            moveQueue.Clear();
            inputDict = InitInputDict();
            inputDict["Reset"] = true;
            APIs.Joypad.Set(inputDict);
            this.reseting = true;
        }
        else
        {
            inputDict = InitInputDict();
            if (this.player2 == false)
            {
                if (PlayerIsActionable(p1Action, this.roundTimer, p1Health, p2Health, executionFrames, airActionAvailable))
                {
                    AddToMoveQueue(action);
                }
                if (moveQueue.Count > 0)
                {
                    string inputForFrame = moveQueue.Dequeue();
                    PerformInput(this.player2 == false, inputDict, inputForFrame, p1FacingLeft == 0);
                }
                if (p1Action == 4)
                {
                    airActionAvailable = false;
                }
                if (p1Action == 0 || p1Action == 2)
                {
                    airActionAvailable = true;
                }
            }
            else
            {
                if (PlayerIsActionable(p2Action, this.roundTimer, p1Health, p2Health, executionFrames, airActionAvailable))
                {
                    AddToMoveQueue(action);
                }
                if (moveQueue.Count > 0)
                {
                    string inputForFrame = moveQueue.Dequeue();
                    PerformInput(this.player2 == false, inputDict, inputForFrame, p2FacingLeft == 0);
                }
                if (p2Action == 4)
                {
                    airActionAvailable = false;
                }
                if (p2Action == 0 || p2Action == 2)
                {
                    airActionAvailable = true;
                }
            }
            if (executionFrames > 0)
            {
                executionFrames -= 1;
            }
        }
        framesElapsed += 1;
    }

    protected override void OnClosed(EventArgs e)
    {
        socketServer.SendToClient("Closing app");
        base.OnClosed(e);
    }

    public long ReadMemory(long addr)
    {
        return APIs.Memory.ReadByte(addr);
    }   
    
    /*
     * Set the input dictionary values and perform the input.
     */
    private void PerformInput(bool player1, Dictionary<string, bool> inputDict, string inputType, bool facingLeft)
    {
        string player = player1 ? "P1" : "P2";
        if (inputType.Contains("+"))
        {
            string[] inputs = inputType.Split('+');
            foreach (string input in inputs)
            {
                switch (input)
                {
                    case ("FORWARD"):
                        if (facingLeft == true)
                        {
                            inputDict[player + " Left"] = true;
                        }
                        else
                        {
                            inputDict[player + " Right"] = true;
                        }
                        break;
                    case ("BACKWARD"):
                        if (facingLeft == true)
                        {
                            inputDict[player + " Right"] = true;
                        }
                        else
                        {
                            inputDict[player + " Left"] = true;
                        }
                        break;
                    case ("UP"):
                        inputDict[player + " Up"] = true;
                        break;
                    case ("DOWN"):
                        inputDict[player + " Down"] = true;
                        break;
                    case ("UPFORWARD"):
                        inputDict[player + " Up"] = true;
                        if (facingLeft == true)
                        {
                            inputDict[player + " Left"] = true;
                        }
                        else
                        {
                            inputDict[player + " Right"] = true;
                        }
                        break;
                    case ("UPBACKWARD"):
                        inputDict[player + " Up"] = true;
                        if (facingLeft == true)
                        {
                            inputDict[player + " Right"] = true;
                        }
                        else
                        {
                            inputDict[player + " Left"] = true;
                        }
                        break;
                    case ("DOWNFORWARD"):
                        inputDict[player + " Down"] = true;
                        if (facingLeft == true)
                        {
                            inputDict[player + " Left"] = true;
                        }
                        else
                        {
                            inputDict[player + " Right"] = true;
                        }
                        break;
                    case ("DOWNBACKWARD"):
                        inputDict[player + " Down"] = true;
                        if (facingLeft == true)
                        {
                            inputDict[player + " Right"] = true;
                        }
                        else
                        {
                            inputDict[player + " Left"] = true;
                        }
                        break;
                    case ("Y"):
                        inputDict[player + " Y"] = true;
                        break;
                    case ("B"):
                        inputDict[player + " B"] = true;
                        break;
                    case ("X"):
                        inputDict[player + " X"] = true;
                        break;
                    case ("A"):
                        inputDict[player + " A"] = true;
                        break;
                    case ("L"):
                        inputDict[player + " L"] = true;
                        break;
                    case ("R"):
                        inputDict[player + " R"] = true;
                        break;
                }
            }
        } 
        else
        {
            switch (inputType)
            {
                case ("FORWARD"):
                    if (facingLeft == true)
                    {
                        inputDict[player + " Left"] = true;
                    }
                    else
                    {
                        inputDict[player + " Right"] = true;
                    }
                    break;
                case ("BACKWARD"):
                    if (facingLeft == true)
                    {
                        inputDict[player + " Right"] = true;
                    }
                    else
                    {
                        inputDict[player + " Left"] = true;
                    }
                    break;
                case ("UP"):
                    inputDict[player + " Up"] = true;
                    break;
                case ("DOWN"):
                    inputDict[player + " Down"] = true;
                    break;
                case ("UPFORWARD"):
                    inputDict[player + " Up"] = true;
                    if (facingLeft == true)
                    {
                        inputDict[player + " Left"] = true;
                    }
                    else
                    {
                        inputDict[player + " Right"] = true;
                    }
                    break;
                case ("UPBACKWARD"):
                    inputDict[player + " Up"] = true;
                    if (facingLeft == true)
                    {
                        inputDict[player + " Right"] = true;
                    }
                    else
                    {
                        inputDict[player + " Left"] = true;
                    }
                    break;
                case ("DOWNFORWARD"):
                    inputDict[player + " Down"] = true;
                    if (facingLeft == true)
                    {
                        inputDict[player + " Left"] = true;
                    }
                    else
                    {
                        inputDict[player + " Right"] = true;
                    }
                    break;
                case ("DOWNBACKWARD"):
                    inputDict[player + " Down"] = true;
                    if (facingLeft == true)
                    {
                        inputDict[player + " Right"] = true;
                    }
                    else
                    {
                        inputDict[player + " Left"] = true;
                    }
                    break;
                case ("Y"):
                    inputDict[player + " Y"] = true;
                    break;
                case ("B"):
                    inputDict[player + " B"] = true;
                    break;
                case ("X"):
                    inputDict[player + " X"] = true;
                    break;
                case ("A"):
                    inputDict[player + " A"] = true;
                    break;
                case ("L"):
                    inputDict[player + " L"] = true;
                    break;
                case ("R"):
                    inputDict[player + " R"] = true;
                    break;
            }
        }
        APIs.Joypad.Set(inputDict);
    }
    
    /*
     * Add the action sent from the client to the current queue of moves to perform.
     * Also set the move execution frames if the move is a multi input special move.
    */
    private void AddToMoveQueue(string action)
    {
        switch(action)
        {
            case ("NONE"):
                moveQueue.Enqueue("none");
                break;
            case ("UP"):
                moveQueue.Enqueue(action);
                break;
            case ("DOWN"):
                moveQueue.Enqueue(action);
                break;
            case ("FORWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("BACKWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("UPFORWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("UPBACKWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("DOWNFORWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("DOWNBACKWARD"):
                moveQueue.Enqueue(action);
                break;
            case ("LPUNCH"):
                moveQueue.Enqueue("Y");
                break;
            case ("MPUNCH"):
                moveQueue.Enqueue("X");
                break;
            case ("HPUNCH"):
                moveQueue.Enqueue("L");
                break;
            case ("LKICK"):
                moveQueue.Enqueue("B");
                break;
            case ("MKICK"):
                moveQueue.Enqueue("A");
                break;
            case ("HKICK"):
                moveQueue.Enqueue("R");
                break;
            case ("CRLPUNCH"):
                moveQueue.Enqueue("DOWN+Y");
                break;
            case ("CRMPUNCH"):
                moveQueue.Enqueue("DOWN+X");
                break;
            case ("CRHPUCNH"):
                moveQueue.Enqueue("DOWN+L");
                break;
            case ("CRLKICK"):
                moveQueue.Enqueue("DOWN+B");
                break;
            case ("CRMKICK"):
                moveQueue.Enqueue("DOWN+A");
                break;
            case ("CRHKICK"):
                moveQueue.Enqueue("DOWN+R");
                break;
            case ("LHADOUKEN"):
                executionFrames = 3;
                foreach (string i in lhadouken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("LSHORYUKEN"):
                executionFrames = 3;
                foreach (string i in lshoryuken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("LTATSU"):
                executionFrames = 3;
                foreach (string i in ltatsu)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("MHADOUKEN"):
                executionFrames = 3;
                foreach (string i in mhadouken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("MSHORYUKEN"):
                executionFrames = 3;
                foreach (string i in mshoryuken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("MTATSU"):
                executionFrames = 3;
                foreach (string i in mtatsu)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("HHADOUKEN"):
                executionFrames = 3;
                foreach (string i in hhadouken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("HSHORYUKEN"):
                executionFrames = 3;
                foreach (string i in hshoryuken)
                {
                    moveQueue.Enqueue(i);
                }
                break;
            case ("HTATSU"):
                executionFrames = 3;
                foreach (string i in htatsu)
                {
                    moveQueue.Enqueue(i);
                }
                break;
        }
    }

    // Check relevant player state to see if inputs can be executed by the player
    private bool PlayerIsActionable(long playerAction, long? roundTimer, long p1Health, long p2Health, int executionFrames, bool airActionAvailable)
    {
        if (playerAction == 4 && airActionAvailable)
        {
            return executionFrames == 0 && RoundActive(roundTimer, p1Health, p2Health);
        }
        else if (playerAction != 4)
        {
            return (playerAction == 0 || playerAction == 2) && executionFrames == 0 && RoundActive(roundTimer, p1Health, p2Health);
        }
        else
        {
            return (playerAction == 0 || playerAction == 4 || playerAction == 2) && executionFrames == 0 && RoundActive(roundTimer, p1Health, p2Health);
        } 
    }

    private bool RoundActive(long? roundTimer, long p1Health, long p2Health)
    {
        return roundTimer > 0 && roundTimer <= 152 && p1Health != 255 && p2Health != 255;
    }

    private Dictionary<string, bool> InitInputDict()
    {
        Dictionary<string, bool> inputDict = new()
        {
            { "P1 Left", false },
            { "P1 Right", false },
            { "P1 Up", false },
            { "P1 Down", false },
            { "P1 Select", false },
            { "P1 Start", false },
            { "P1 Y", false },
            { "P1 B", false },
            { "P1 X", false },
            { "P1 A", false },
            { "P1 L", false },
            { "P1 R", false }
        };
        return inputDict;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _ = socketServer?.StopServer();
        base.OnFormClosed(e);
    }
}