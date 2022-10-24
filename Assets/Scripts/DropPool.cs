using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.U2D;

public class DropPool : MonoBehaviour
{
    public IObjectPool<Drop> MyDropPool { get; private set; }
    public int RandomIndex;
    public Drop[] Drops;

    [SerializeField] SpriteAtlas _atlas;

    void Awake()
    {
        //Initializing drop pool
        MyDropPool = new ObjectPool<Drop>
            (
            CreateDrop,
            ActionOnGet,
            ActionOnRelease,
            ActionOnDestory
            );
    }

    Drop CreateDrop()
    {
        Drop drop = Instantiate(Drops[RandomIndex], transform);

        drop.SetPool(MyDropPool);

        return drop;
    }

    void ActionOnGet(Drop drop)
    {
        drop.gameObject.SetActive(true);
    }

    void ActionOnRelease(Drop drop)
    {
        drop.gameObject.SetActive(false);

        int random = Random.Range(0, _atlas.spriteCount);

        drop.GetComponent<Drop>().IsMatched = false;
        drop.transform.localScale = Drops[0].transform.localScale;

        SpriteRenderer dropRenderer = drop.GetComponent<SpriteRenderer>();
        dropRenderer.color = Color.white;

        //Get random drop after return to pool for next call
        if (random == 0)
        {
            dropRenderer.sprite = _atlas.GetSprite("dropB");
            drop.tag = "BlueDrop";
        }
        else if (random == 1)
        {
            dropRenderer.sprite = _atlas.GetSprite("dropG");
            drop.tag = "GreenDrop";
        }
        else if (random == 2)
        {
            dropRenderer.sprite = _atlas.GetSprite("dropR");
            drop.tag = "RedDrop";
        }
        else if (random == 3)
        {
            dropRenderer.sprite = _atlas.GetSprite("dropY");
            drop.tag = "YellowDrop";
        }
    }

    void ActionOnDestory(Drop drop)
    {
        Destroy(drop.gameObject);
    }
}
