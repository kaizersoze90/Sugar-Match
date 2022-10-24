using UnityEngine;
using UnityEngine.Pool;

public class Drop : MonoBehaviour
{
    [Header("General Settings")]
    public int Column;
    public int Row;
    public int PreviousColumn;
    public int PreviousRow;
    public bool IsMatched;

    BoardManager _board;
    GameObject _otherDrop;
    IObjectPool<Drop> _dropPool;

    void OnEnable()
    {
        _board = FindObjectOfType<BoardManager>();
    }

    void Start()
    {
        PreviousColumn = Column;
        PreviousRow = Row;
    }

    public void SetPool(IObjectPool<Drop> pool)
    {
        _dropPool = pool;
    }

    public void ReturnToPool()
    {
        _dropPool.Release(this);
    }

    public void ProcessSwipe(float swipeAngle)
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && Column < _board.Width - 1)
        {
            //RIGHT SWIPE
            //Get other drop and adjust new position
            _otherDrop = _board.AllDrops[Column + 1, Row];
            if (_otherDrop == null) { return; }
            _otherDrop.GetComponent<Drop>().Column--;
            _otherDrop.name = $"({Column}, {Row})";
            _board.AllDrops[Column, Row] = _otherDrop;

            //Adjust this drop after new position
            Column++;
            _board.AllDrops[Column, Row] = gameObject;
            gameObject.name = $"({Column}, {Row})";

            //Process swap
            _board.SwapPieces(gameObject, new Vector2(Column, Row),
                              _otherDrop, new Vector2(Column - 1, Row));
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && Row < _board.Height - 1)
        {
            //UP SWIPE
            //Get other drop and adjust new position
            _otherDrop = _board.AllDrops[Column, Row + 1];
            if (_otherDrop == null) { return; }
            _otherDrop.GetComponent<Drop>().Row--;
            _otherDrop.name = $"({Column}, {Row})";
            _board.AllDrops[Column, Row] = _otherDrop;

            //Adjust this drop after new position
            Row++;
            _board.AllDrops[Column, Row] = gameObject;
            gameObject.name = $"({Column}, {Row})";

            //Process swap
            _board.SwapPieces(gameObject, new Vector2(Column, Row),
                              _otherDrop, new Vector2(Column, Row - 1));
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && Column > 0)
        {
            //LEFT SWIPE
            //Get other drop and adjust new position
            _otherDrop = _board.AllDrops[Column - 1, Row];
            if (_otherDrop == null) { return; }
            _otherDrop.GetComponent<Drop>().Column++;
            _otherDrop.name = $"({Column}, {Row})";
            _board.AllDrops[Column, Row] = _otherDrop;

            //Adjust this drop after new position
            Column--;
            _board.AllDrops[Column, Row] = gameObject;
            gameObject.name = $"({Column}, {Row})";

            //Process swap
            _board.SwapPieces(gameObject, new Vector2(Column, Row),
                              _otherDrop, new Vector2(Column + 1, Row));
        }
        else if (swipeAngle > -135 && swipeAngle <= -45 && Row > 0)
        {
            //DOWN SWIPE
            //Get other drop and adjust new position
            _otherDrop = _board.AllDrops[Column, Row - 1];
            if (_otherDrop == null) { return; }
            _otherDrop.GetComponent<Drop>().Row++;
            _otherDrop.name = $"({Column}, {Row})";
            _board.AllDrops[Column, Row] = _otherDrop;

            //Adjust this drop after new position
            Row--;
            _board.AllDrops[Column, Row] = gameObject;
            gameObject.name = $"({Column}, {Row})";

            //Process swap
            _board.SwapPieces(gameObject, new Vector2(Column, Row),
                              _otherDrop, new Vector2(Column, Row + 1));
        }
    }
}
