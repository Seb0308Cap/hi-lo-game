namespace HiLoGame.Models.Events;

public record PlayersReadyForNextGameEvent(
    List<string> ReadyPlayerNames,
    List<string> WaitingPlayerNames,
    bool AllReady
);
