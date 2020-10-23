﻿using UnityEngine;

public class Global : MonoBehaviour
{

  /**** Dependency ****/
  // Board or awaiting pieces to enter in game or that.
  public GameObject whitePiecePrefab;
  public GameObject blackPiecePrefab;


  /**** Relative to Game Object and View ****/
  // Unique set of Pieces.
  public Piece[,] pieces = new Piece[8, 8];
  public Piece[,] newPiecesNotOnBoard = new Piece[2, 8];
  public int Player = 0;
  // Board du DamiersEcce.
  private Board_Ecce Board_Ecce;
  /**** Action ****/
  Vector2 mouseOver;
  Vector2 startDrag;
  int ScorePlayer1 = 0;
  int ScorePlayer2 = 0;

  private readonly int caseLength = 2;

  /**** View ****/
  private Piece selectedPiece;

  /*
   * Gather all components.
   * Generate Pieces.
   */
  public void Awake()
  {
    Board_Ecce = GetComponentInChildren<Board_Ecce>();
    this.GeneratePieces();
  }

  /*
   * Allow us to detect click, and drag and drop event.
   */
  private void Update()
  {
    this.UpdateMouseOver();
  }

  /*
   * Once the user ask to move a Piece
   */
  private void UpdateMouseOver()
  {
    if (Camera.main && Input.GetMouseButtonDown(0))
    {
      // Getting physics
      bool physicsBoardEcce = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
         out RaycastHit hit, 25.0f, LayerMask.GetMask("Board_Ecce"));
      if (physicsBoardEcce)
      {
        // Saving mouseOver
        mouseOver.x = (int)hit.point.x;
        mouseOver.y = (int)hit.point.z;
        if (Input.GetMouseButtonDown(0))
        {
          if (selectedPiece == null)
          {
            // Selecting piece
            TrySelectPiece(mouseOver);
          }
          else {
            Piece advPiece = GetPiece(mouseOver);
            if (advPiece != null && !advPiece.name.Contains(this.selectedPiece.name.Substring(0, 5)))
            {
              // Eating piece
              if (GameLogic.IsMovePossible(true, selectedPiece.isEcce,
                ToBoardCoordinates(startDrag), ToBoardCoordinates(mouseOver)))
              {
                RemovingPiece(ToArrayCoordinates(mouseOver));
                TryMovePiece(selectedPiece, mouseOver, startDrag);

              }
            }
            else
            {
              // Moving piece
              // If there is no piece on the case && if it is a possible move
              if(GetPiece(mouseOver) == null && GameLogic.IsMovePossible(selectedPiece.isEcce, true,
                ToBoardCoordinates(startDrag), ToBoardCoordinates(mouseOver)))
              {
                TryMovePiece(selectedPiece, mouseOver, startDrag);
              }
            }
          }
        }
      }
    }
  }

  private Piece GetPiece(Vector2 position)
  {
    // Getting the piece out of the array
    return pieces[
      (int)ToArrayCoordinates(position).x,
      (int)ToArrayCoordinates(position).y
      ];
  }

  private void TrySelectPiece(Vector2 mouseOver)
  {
    selectedPiece = GetPiece(mouseOver);
    startDrag = mouseOver;
    // Showing selection
    if (selectedPiece != null)
    {
      // Material selection
      selectedPiece.GetComponent<MeshRenderer>().material = selectedPiece.myMaterials[1];
      // Perspective selection
      selectedPiece.gameObject.transform.position =
        (Vector3.right * ToBoardCoordinates(mouseOver).x) +
        (Vector3.forward * ToBoardCoordinates(mouseOver).y) +
        (Vector3.up * 1.1f);
    }
  }

  public Vector2 ToArrayCoordinates(Vector2 c)
  {
    return new Vector2(
      Mathf.FloorToInt(c.x / caseLength),
      Mathf.FloorToInt(c.y / caseLength)
   );
  }

  public Vector2 ToBoardCoordinates(Vector2 c)
  {
    return new Vector2(
      Mathf.FloorToInt(c.x / caseLength) * caseLength + caseLength / 2,
      Mathf.FloorToInt(c.y / caseLength) * caseLength + caseLength / 2
    );
  }

  public void TryMovePiece(Piece p, Vector2 mouseOver, Vector2 startDrag)
  {
    // Moving piece
    p.transform.position =
      (Vector3.right * ToBoardCoordinates(mouseOver).x) +
      (Vector3.forward * ToBoardCoordinates(mouseOver).y) +
      (Vector3.up * 1);
    // Deleting piece in array
    pieces[
      (int)ToArrayCoordinates(startDrag).x,
      (int)ToArrayCoordinates(startDrag).y
      ] = null;
    // Placing piece in array
    pieces[
      (int)ToArrayCoordinates(mouseOver).x,
      (int)ToArrayCoordinates(mouseOver).y
      ] = p;
    // In case of a first piece move
    TryPlaceNewPiece(Player);
    CheckPieceEvolution(p, ToArrayCoordinates(mouseOver));
    FinishTurn();
  }


  /*
   * Set of board of Entries Generator for both types.
   */
  private void GeneratePieces()
  {
    pieces[1, 1] = GeneratePiece(whitePiecePrefab,
      new Vector2(1 * caseLength + 1, 1 * caseLength + 1));
    pieces[6, 1] = GeneratePiece(blackPiecePrefab,
      new Vector2(6 * caseLength + 1, 1 * caseLength + 1));
    for (var i = 0; i < 2; i++)
    {
      for (var j = 0; j < 7; j++)
      {
        Piece piece;
        if (i % 2 == 0)
        {
          piece = GeneratePiece(whitePiecePrefab,
      ToBoardCoordinates(new Vector2(1 * caseLength + 1, 1 * caseLength)));
        } else
        {
          piece = GeneratePiece(blackPiecePrefab,
      ToBoardCoordinates(new Vector2(6 * caseLength + 1, 1 * caseLength)));
        }
        newPiecesNotOnBoard[i, j] = piece;
        startDrag = ToBoardCoordinates(new Vector2(-2 * caseLength + 1, 5 * caseLength));
      }
    }
  }

  /**
  * Single Piece Generator.
  */
  private Piece GeneratePiece(GameObject piecePrefab, Vector2 coordinate)
  {
    GameObject go = Instantiate(piecePrefab) as GameObject;
    go.AddComponent<Piece>();
    go.transform.SetParent(Board_Ecce.transform);
    Piece piece = go.GetComponent<Piece>();
    piece.transform.position =
      (Vector3.right * coordinate.x) +
      (Vector3.forward * coordinate.y) +
      (Vector3.up * 1);
    return piece;
  }

  private Piece GetANewPiece(int Player)
  {
    var i = 0;
    var found = false;
    while (i < 7 && !found)
    {
      if (newPiecesNotOnBoard[Player, i] != null)
      {
        found = true;
      }
      else
      {
        i++;
      }
    }
    Piece piece = newPiecesNotOnBoard[Player, i];
    newPiecesNotOnBoard[Player, i] = null;
    return piece;
  }

  public void FinishTurn()
  {
    if (Player == 0)
    {
      Player = 1;
    } else
    {
      Player = 0;
    }
    startDrag = mouseOver;
    mouseOver = new Vector2();
    selectedPiece.GetComponent<MeshRenderer>().material = selectedPiece.myMaterials[0];
    selectedPiece = null;
  }

  public void TryPlaceNewPiece(int player)
  {
    Piece p = GetANewPiece(Player);
    if(Player == 0) {
      pieces[1, 1] = p;
    } else
    {
      pieces[6, 1] = p;
    }
  }

  public void RemovingPiece(Vector2 mouseOver)
  {
    pieces[(int)mouseOver.x, (int)mouseOver.y].gameObject.SetActive(false);
    pieces[(int)mouseOver.x, (int)mouseOver.y] = null;
  }

  public Piece CheckPieceEvolution(Piece p, Vector2 mouseOver)
  {
    if(mouseOver.x == 1 && mouseOver.y == 6 && p.name.Contains("white")
      ||
       mouseOver.x == 6 && mouseOver.y == 6 && p.name.Contains("black")
      )
    {
      p.isEcce = true;
    }
    return p;
  }

  /** When First Piece is moved, regenerate one **/

}