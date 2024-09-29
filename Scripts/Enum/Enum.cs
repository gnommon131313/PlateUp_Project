public enum Conversion // возможный импакт конвертации продукта
{
    None = 1 << 0,
    Fry = 1 << 1,
    Cut = 1 << 2,
    Wash = 1 << 3,
    Crush = 1 << 31,
}

public enum GameState // состояния игры
{
    InWorld = 1 << 0,
    InBeginGame = 1 << 1,
    InGame = 1 << 2,
    InGameOver = 1 << 3,
    InPause = 1 << 31,
}

public enum UIVisibleCondition // когда видно этот элемент UI
{
    InWorld = 1 << 0,
    InBeginGame = 1 << 1,
    InGame = 1 << 2,
    InGameOver = 1 << 3,
    InPause = 1 << 29,
    Never = 1 << 30,
    Always = 1 << 31, 
}
