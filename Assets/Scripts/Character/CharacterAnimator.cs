using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> IdleDownSprites;
    [SerializeField] List<Sprite> IdleUpSprites;
    [SerializeField] List<Sprite> IdleRightSprites;
    [SerializeField] List<Sprite> IdleLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;


    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    // States
    SpriteAnimator IdleDownAnim;
    SpriteAnimator IdleUpAnim;
    SpriteAnimator IdleRightAnim;
    SpriteAnimator IdleLeftAnim;

    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;

    // Refrences
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        IdleDownAnim = new SpriteAnimator(IdleDownSprites, spriteRenderer);
        IdleUpAnim = new SpriteAnimator(IdleUpSprites, spriteRenderer);
        IdleRightAnim = new SpriteAnimator(IdleRightSprites, spriteRenderer);
        IdleLeftAnim = new SpriteAnimator(IdleLeftSprites, spriteRenderer);

        currentAnim = IdleDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
            currentAnim = walkRightAnim;
        else if (MoveX == -1)
            currentAnim = walkLeftAnim;
        else if (MoveY == 1)
            currentAnim = walkUpAnim;
        else if (MoveY == -1)
            currentAnim = walkDownAnim;

        if (IsMoving)
            currentAnim.HandleUpdate();
        else
        {
            if (MoveX > 0)
            {
                currentAnim = IdleRightAnim;
            }
            else if (MoveX < 0)
            {
                currentAnim = IdleLeftAnim;
            }
            else if (MoveY > 0)
            {
                currentAnim = IdleUpAnim;
            }
            else
            {
                currentAnim = IdleDownAnim;
            }
            currentAnim.HandleUpdate();
        }
        wasPreviouslyMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if (dir == FacingDirection.Right)
            MoveX = 2;
        else if (dir == FacingDirection.Left)
            MoveX = -2;
        else if (dir == FacingDirection.Down)
            MoveY = -2;
        else if (dir == FacingDirection.Up)
            MoveY = 2;
    }

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }
}

public enum FacingDirection { Up, Down, Left, Right }
