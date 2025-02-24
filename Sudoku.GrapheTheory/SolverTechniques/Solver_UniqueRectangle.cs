﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.ResolutionTechniquesHumaines;

partial class Solver
{
	private bool UniqueRectangle()
	{
		for (int type = 1; type <= 6; type++) // Type
		{
			for (int x1 = 0; x1 < 9; x1++)
			{
				Region c1 = Puzzle.ColumnsI[x1];
				for (int x2 = x1 + 1; x2 < 9; x2++)
				{
					Region c2 = Puzzle.ColumnsI[x2];
					for (int y1 = 0; y1 < 9; y1++)
					{
						for (int y2 = y1 + 1; y2 < 9; y2++)
						{
							for (int value1 = 1; value1 <= 9; value1++)
							{
								for (int value2 = value1 + 1; value2 <= 9; value2++)
								{
									int[] candidates = [value1, value2];
									Cell[] cells = [c1[y1], c1[y2], c2[y1], c2[y2]];
									if (cells.Any(c => !c.CandI.ContainsAll(candidates)))
									{
										continue;
									}

									ILookup<int, Cell> l = cells.ToLookup(c => c.CandI.Count);
									Cell[] gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g).ToArray(),
											two = l[2].ToArray(), three = l[3].ToArray(), four = l[4].ToArray();

									switch (type) // Check for candidate counts
									{
										case 1:
										{
											if (two.Length != 3 || gtTwo.Length != 1)
											{
												continue;
											}
											break;
										}
										case 2:
										case 6:
										{
											if (two.Length != 2 || three.Length != 2)
											{
												continue;
											}
											break;
										}
										case 3:
										{
											if (two.Length != 2 || gtTwo.Length != 2)
											{
												continue;
											}
											break;
										}
										case 4:
										{
											if (two.Length != 2 || three.Length != 1 || four.Length != 1)
											{
												continue;
											}
											break;
										}
										case 5:
										{
											if (two.Length != 1 || three.Length != 3)
											{
												continue;
											}
											break;
										}
									}

									switch (type) // Check for extra rules
									{
										case 1:
										{
											if (gtTwo[0].CandI.Count == 3)
											{
												gtTwo[0].SetValue(gtTwo[0].CandI.Single(c => !candidates.Contains(c)));
											}
											else
											{
												Cell.ChangeCandidates(gtTwo, candidates);
											}
											break;
										}
										case 2:
										{
											if (!three[0].CandI.SetEquals(three[1].CandI))
											{
												continue;
											}
											if (!Cell.ChangeCandidates(three[0].VisibleCells.Intersect(three[1].VisibleCells), three[0].CandI.Except(candidates)))
											{
												continue;
											}
											break;
										}
										case 3:
										{
											if (gtTwo[0].Point.Column != gtTwo[1].Point.Column && gtTwo[0].Point.Row != gtTwo[1].Point.Row)
											{
												continue; // Must be non-diagonal
											}
											IEnumerable<int> others = gtTwo[0].CandI.Except(candidates).Union(gtTwo[1].CandI.Except(candidates));
											if (others.Count() > 4 || others.Count() < 2)
											{
												continue;
											}
											IEnumerable<Cell> nSubset = ((gtTwo[0].Point.Row == gtTwo[1].Point.Row) ? // Same row
														Puzzle.RowsI[gtTwo[0].Point.Row] : Puzzle.ColumnsI[gtTwo[0].Point.Column])
														.Where(c => c.CandI.ContainsAny(others) && !c.CandI.ContainsAny(Utils.OneToNine.Except(others)));
											if (nSubset.Count() != others.Count() - 1)
											{
												continue;
											}
											if (!Cell.ChangeCandidates(nSubset.Union(gtTwo).Select(c => c.VisibleCells).IntersectAll(), others))
											{
												continue;
											}
											break;
										}
										case 4:
										{
											int[] remove = new int[1];
											if (four[0].Point.BlockIndex == three[0].Point.BlockIndex)
											{
												if (Puzzle.BlocksI[four[0].Point.BlockIndex].GetCellsWithCandidate(value1).Count() == 2)
												{
													remove[0] = value2;
												}
												else if (Puzzle.BlocksI[four[0].Point.BlockIndex].GetCellsWithCandidate(value2).Count() == 2)
												{
													remove[0] = value1;
												}
											}
											if (remove[0] != 0) // They share the same row/column but not the same block
											{
												if (three[0].Point.Column == three[0].Point.Column)
												{
													if (Puzzle.ColumnsI[four[0].Point.Column].GetCellsWithCandidate(value1).Count() == 2)
													{
														remove[0] = value2;
													}
													else if (Puzzle.ColumnsI[four[0].Point.Column].GetCellsWithCandidate(value2).Count() == 2)
													{
														remove[0] = value1;
													}
												}
												else
												{
													if (Puzzle.RowsI[four[0].Point.Row].GetCellsWithCandidate(value1).Count() == 2)
													{
														remove[0] = value2;
													}
													else if (Puzzle.RowsI[four[0].Point.Row].GetCellsWithCandidate(value2).Count() == 2)
													{
														remove[0] = value1;
													}
												}
											}
											else
											{
												continue;
											}
											Cell.ChangeCandidates(cells.Except(l[2]), remove);
											break;
										}
										case 5:
										{
											if (!three[0].CandI.SetEquals(three[1].CandI) || !three[1].CandI.SetEquals(three[2].CandI))
											{
												continue;
											}
											if (!Cell.ChangeCandidates(three.Select(c => c.VisibleCells).IntersectAll(), three[0].CandI.Except(candidates)))
											{
												continue;
											}
											break;
										}
										case 6:
										{
											if (three[0].Point.Column == three[1].Point.Column)
											{
												continue;
											}
											int set;
											if (c1.GetCellsWithCandidate(value1).Count() == 2 && c2.GetCellsWithCandidate(value1).Count() == 2 // Check if "v" only appears in the UR
												&& Puzzle.RowsI[two[0].Point.Row].GetCellsWithCandidate(value1).Count() == 2
													&& Puzzle.RowsI[two[1].Point.Row].GetCellsWithCandidate(value1).Count() == 2)
											{
												set = value1;
											}
											else if (c1.GetCellsWithCandidate(value2).Count() == 2 && c2.GetCellsWithCandidate(value2).Count() == 2
												&& Puzzle.RowsI[two[0].Point.Row].GetCellsWithCandidate(value2).Count() == 2
													&& Puzzle.RowsI[two[1].Point.Row].GetCellsWithCandidate(value2).Count() == 2)
											{
												set = value2;
											}
											else
											{
												continue;
											}
											two[0].SetValue(set);
											two[1].SetValue(set);
											break;
										}
									}

									LogAction(TechniqueFormat("Unique rectangle",
										"{0}: {1}",
										Utils.PrintCells(cells), Utils.PrintCandidates(candidates)),
										(ReadOnlySpan<Cell>)cells);
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}
}
