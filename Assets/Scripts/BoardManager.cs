using DG.Tweening;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardManager : MonoBehaviour
{
    public GameObject[,] AllDrops;
    public int Width;
    public int Height;

    [Header("General Components")]
    [SerializeField] GameObject _tilePrefab;
    [SerializeField] AudioClip _destroySFX;

    [Tooltip("Starts from 0. For example: If you wish to disable second column, enter 1")]
    [SerializeField] int[] _disabledRefillColumns;

    DropPool _pool;

    void Start()
    {
        AllDrops = new GameObject[Width, Height];

        _pool = GetComponent<DropPool>();

        SetUp();
    }

    public void SwapPieces(GameObject firstDrop, Vector2 firstPos, GameObject otherDrop, Vector2 otherPos)
    {
        //Move touched drop and process matches on complete
        firstDrop.transform.DOMove(firstPos, 0.3f).SetEase(Ease.InOutSine)
                          .OnComplete(() =>
                          {
                              CheckAllMatches();
                              AdjustLeftoverDrop(firstDrop, otherDrop);
                              ProcessWrongMatches(firstDrop, otherDrop);
                          });
        firstDrop.transform.DOPunchScale(new Vector3(1f, 1f, 1f), 0.3f, 1);

        //Swap other drop position
        otherDrop.transform.DOMove(otherPos, 0.3f).SetEase(Ease.InOutSine);
    }

    void CheckAllMatches()
    {
        foreach (GameObject piece in AllDrops)
        {
            if (piece != null)
            {
                Drop drop = piece.GetComponent<Drop>();

                //Check for 3 same match in column
                if (drop.Column > 0 && drop.Column < Width - 1)
                {
                    GameObject leftDrop1 = AllDrops[drop.Column - 1, drop.Row];
                    GameObject rightDrop1 = AllDrops[drop.Column + 1, drop.Row];

                    string myTag = piece.tag;

                    //IF ITS MATCH
                    if (leftDrop1 != null && rightDrop1 != null)
                    {
                        if (leftDrop1.CompareTag(myTag) && rightDrop1.CompareTag(myTag))
                        {
                            leftDrop1.GetComponent<Drop>().IsMatched = true;
                            rightDrop1.GetComponent<Drop>().IsMatched = true;
                            drop.IsMatched = true;

                            ChangeColors(piece, leftDrop1, rightDrop1);
                        }
                    }
                }

                //Check for 3 same match in row
                if (drop.Row > 0 && drop.Row < Height - 1)
                {
                    GameObject upDrop1 = AllDrops[drop.Column, drop.Row + 1];
                    GameObject downDrop1 = AllDrops[drop.Column, drop.Row - 1];

                    string myTag = piece.tag;

                    //IF ITS MATCH
                    if (upDrop1 != null && downDrop1 != null)
                    {
                        if (upDrop1.CompareTag(myTag) && downDrop1.CompareTag(myTag))
                        {
                            upDrop1.GetComponent<Drop>().IsMatched = true;
                            downDrop1.GetComponent<Drop>().IsMatched = true;
                            drop.IsMatched = true;

                            ChangeColors(piece, upDrop1, downDrop1);
                        }
                    }
                }
            }
        }

        //Start destroy matches for (IsMatched=true) drops
        StartCoroutine(DestoryMatches());
    }

    void AdjustLeftoverDrop(GameObject firstPiece, GameObject otherPiece)
    {
        //Adjust leftover drop previous positions that if helped to a successful match
        Drop firstDrop = firstPiece.GetComponent<Drop>();
        Drop otherDrop = otherPiece.GetComponent<Drop>();

        if (firstDrop.IsMatched)
        {
            otherDrop.PreviousColumn = otherDrop.Column;
            otherDrop.PreviousRow = otherDrop.Row;
        }
        else if (otherDrop.IsMatched)
        {
            firstDrop.PreviousColumn = firstDrop.Column;
            firstDrop.PreviousRow = firstDrop.Row;
        }
    }

    void ProcessWrongMatches(GameObject firstPiece, GameObject otherPiece)
    {
        Drop firstDrop = firstPiece.GetComponent<Drop>();
        Drop otherDrop = otherPiece.GetComponent<Drop>();

        //IF ITS NOT MATCH
        if (!firstDrop.IsMatched && !otherDrop.IsMatched)
        {
            //Reset other drop to previous position
            otherDrop.Column = otherDrop.PreviousColumn;
            otherDrop.Row = otherDrop.PreviousRow;
            otherDrop.gameObject.name = $"({otherDrop.Column}, {otherDrop.Row})";
            AllDrops[otherDrop.Column, otherDrop.Row] = otherDrop.gameObject;

            //Reset first drop to previous position
            firstDrop.Column = firstDrop.PreviousColumn;
            firstDrop.Row = firstDrop.PreviousRow;
            firstDrop.gameObject.name = $"({firstDrop.Column}, {firstDrop.Row})";
            AllDrops[firstDrop.Column, firstDrop.Row] = firstDrop.gameObject;

            //Play animations
            firstPiece.transform.DOMove(new Vector2(firstDrop.Column, firstDrop.Row), 0.3f)
                                .SetEase(Ease.InOutSine);
            firstPiece.transform.DOPunchScale(new Vector3(1f, 1f, 1f), 0.3f, 1);

            otherPiece.transform.DOMove(new Vector2(otherDrop.Column, otherDrop.Row), 0.3f)
                                .SetEase(Ease.InOutSine);
        }
    }

    IEnumerator DestoryMatches()
    {
        int destroyedDrop = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (AllDrops[i, j] != null && AllDrops[i, j].GetComponent<Drop>().IsMatched)
                {
                    Drop drop = AllDrops[i, j].GetComponent<Drop>();

                    //Play animation for IsMatched=true drop and return to pool on complete
                    drop.transform.DOPunchScale(new Vector2(-1, -1), 0.2f, 1)
                                  .OnComplete(() => drop.ReturnToPool());

                    AllDrops[i, j] = null;

                    yield return new WaitForSeconds(0.05f);

                    destroyedDrop++;
                }
            }
        }

        //If there are any destroyed matches, collapse columns
        if (destroyedDrop > 0)
        {
            StartCoroutine(CollapseColumn());

            AudioSource.PlayClipAtPoint(_destroySFX, Camera.main.transform.position);
        }
    }

    IEnumerator CollapseColumn()
    {
        int nullCount = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                //If there is a empty tile, increase count for move down
                if (AllDrops[i, j] == null)
                {
                    nullCount++;
                }
                //Move down drops as much as null count
                else if (nullCount > 0)
                {
                    Drop drop = AllDrops[i, j].GetComponent<Drop>();

                    drop.Row -= nullCount;
                    drop.PreviousRow = drop.Row;
                    drop.name = $"({drop.Column}, {drop.Row})";

                    drop.transform.DOMove(new Vector2(drop.Column, drop.Row), 0.3f)
                                  .SetEase(Ease.InOutSine);

                    AllDrops[i, j] = null;
                    AllDrops[drop.Column, drop.Row] = drop.gameObject;
                }
            }
            //And reset null count on loop complete
            nullCount = 0;
        }

        yield return new WaitForSeconds(0.5f);

        RefillBoard();

        yield return new WaitForSeconds(0.5f);

        CheckAllMatches();
    }

    void RefillBoard()
    {
        for (int i = 0; i < Width; i++)
        {
            //If related column spawner is disabled then go to next column
            foreach (int c in _disabledRefillColumns)
            {
                if (i == c) { goto NextLoop; }
            }

            //Otherwise check for empty tiles and fill them with new ones
            for (int j = 0; j < Height; j++)
            {
                if (AllDrops[i, j] == null)
                {
                    Vector2 targetPos = new Vector2(i, j);
                    Vector2 offsetPos = new Vector2(i, 12);

                    Drop drop = _pool.MyDropPool.Get();
                    drop.transform.position = offsetPos;
                    drop.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutSine);
                    drop.name = $"({i}, {j})";
                    drop.Column = i;
                    drop.PreviousColumn = i;
                    drop.Row = j;
                    drop.PreviousRow = j;

                    AllDrops[i, j] = drop.gameObject;
                }
            }
        NextLoop:
            continue;
        }
    }

    void SetUp()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                //Create background tile
                Vector2 targetPos = new Vector2(i, j);
                GameObject tile = Instantiate(_tilePrefab, targetPos, Quaternion.identity, transform);
                tile.name = $"--({i}, {j})";

                //Create random drops
                int random = Random.Range(0, _pool.Drops.Length);
                int maxIterations = 0;

                //If there are repetitive drops, recalculate random number
                while (PreventRepetitiveDrop(i, j, _pool.Drops[random]) && maxIterations < 100)
                {
                    random = Random.Range(0, _pool.Drops.Length);
                    maxIterations++;
                }

                //Set correct random index for pool
                _pool.RandomIndex = random;

                //Get drop from pool and adjust settings
                Drop drop = _pool.MyDropPool.Get();
                drop.transform.position = targetPos;
                drop.name = $"({i}, {j})";
                drop.Column = i;
                drop.Row = j;

                AllDrops[i, j] = drop.gameObject;
            }
        }
    }

    bool PreventRepetitiveDrop(int column, int row, Drop drop)
    {
        //Checking for three same drop in a column and row
        if (column > 1 & row > 1)
        {
            if (AllDrops[column - 1, row].CompareTag(drop.tag) &&
                AllDrops[column - 2, row].CompareTag(drop.tag))
            {
                return true;
            }

            if (AllDrops[column, row - 1].CompareTag(drop.tag) &&
                AllDrops[column, row - 2].CompareTag(drop.tag))
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (AllDrops[column, row - 1].CompareTag(drop.tag) &&
                    AllDrops[column, row - 2].CompareTag(drop.tag))
                {
                    return true;
                }
            }

            if (column > 1)
            {
                if (AllDrops[column - 1, row].CompareTag(drop.tag) &&
                    AllDrops[column - 2, row].CompareTag(drop.tag))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void ChangeColors(GameObject match1, GameObject match2, GameObject match3)
    {
        match1.GetComponent<SpriteRenderer>().color = Color.gray;
        match2.GetComponent<SpriteRenderer>().color = Color.gray;
        match3.GetComponent<SpriteRenderer>().color = Color.gray;
    }
}
