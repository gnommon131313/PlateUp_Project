using System;

public class OrderExpired
{
}

public class MapLoaded
{
    public int Index { get; private set; }

    public MapLoaded(int index)
    {
        Index = index;
    }
}

public class GameOver
{
}

public class GameBegins
{
}

public class MapSwitcherOpen
{
    public int Index { get; private set; }

    public MapSwitcherOpen(int index)
    {
        Index = index;
    }
}

public class MapSwitcherClose
{
}