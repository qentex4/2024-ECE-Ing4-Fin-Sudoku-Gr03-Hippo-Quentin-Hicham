﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.ResolutionTechniquesHumaines;

partial class Solver
{
	private bool AvoidableRectangle()
	{
		for (int type = 1; type <= 2; type++)
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
									if (cells.Any(c => c.OriginalValue != Cell.EMPTY_VALUE))
									{
										continue;
									}

									IEnumerable<Cell> alreadySet = cells.Where(c => c.Value != Cell.EMPTY_VALUE);
									IEnumerable<Cell> notSet = cells.Where(c => c.Value == Cell.EMPTY_VALUE);

									switch (type)
									{
										case 1:
										{
											if (alreadySet.Count() != 3)
											{
												continue;
											}
											break;
										}
										case 2:
										{
											if (alreadySet.Count() != 2)
											{
												continue;
											}
											break;
										}
									}
									Cell[][] pairs =
									[
										[cells[0], cells[3]],
										[cells[1], cells[2]]
									];
									foreach (Cell[] pair in pairs)
									{
										Cell[] otherPair = pair == pairs[0] ? pairs[1] : pairs[0];
										foreach (int i in candidates)
										{
											int otherVal = candidates.Single(ca => ca != i);
											if (((pair[0].Value == i && pair[1].Value == Cell.EMPTY_VALUE && pair[1].CandI.Count == 2 && pair[1].CandI.Contains(i))
												|| (pair[1].Value == i && pair[0].Value == Cell.EMPTY_VALUE && pair[0].CandI.Count == 2 && pair[0].CandI.Contains(i)))
												&& otherPair.All(c => c.Value == otherVal || (c.CandI.Count == 2 && c.CandI.Contains(otherVal))))
											{
												goto breakpairs;
											}
										}
									}
									continue; // Did not find
								breakpairs:
									bool changed = false;
									switch (type)
									{
										case 1:
										{
											Cell cell = notSet.ElementAt(0);
											if (cell.CandI.Count == 2)
											{
												cell.SetValue(cell.CandI.Except(candidates).ElementAt(0));
											}
											else
											{
												cell.CandI.Set(cell.CandI.Intersect(candidates), false);
											}
											changed = true;
											break;
										}
										case 2:
										{
											IEnumerable<int> commonCandidates = notSet.Select(c => c.CandI.Except(candidates)).IntersectAll();
											if (commonCandidates.Any()
												&& Cell.ChangeCandidates(notSet.Select(c => c.VisibleCells).IntersectAll(), commonCandidates))
											{
												changed = true;
											}
											break;
										}
									}

									if (changed)
									{
										LogAction(TechniqueFormat("Avoidable rectangle",
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
		}
		return false;
	}
}
