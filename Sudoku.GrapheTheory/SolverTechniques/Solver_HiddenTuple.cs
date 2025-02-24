﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.ResolutionTechniquesHumaines;

partial class Solver
{
	private bool HiddenQuadruple()
	{
		for (int i = 0; i < 9; i++)
		{
			if (HiddenTuple_Find(Puzzle.BlocksI[i], 4)
				|| HiddenTuple_Find(Puzzle.RowsI[i], 4)
				|| HiddenTuple_Find(Puzzle.ColumnsI[i], 4))
			{
				return true;
			}
		}
		return false;
	}

	private bool HiddenTriple()
	{
		for (int i = 0; i < 9; i++)
		{
			if (HiddenTuple_Find(Puzzle.BlocksI[i], 3)
				|| HiddenTuple_Find(Puzzle.RowsI[i], 3)
				|| HiddenTuple_Find(Puzzle.ColumnsI[i], 3))
			{
				return true;
			}
		}
		return false;
	}

	private bool HiddenPair()
	{
		for (int i = 0; i < 9; i++)
		{
			if (HiddenTuple_Find(Puzzle.BlocksI[i], 2)
				|| HiddenTuple_Find(Puzzle.RowsI[i], 2)
				|| HiddenTuple_Find(Puzzle.ColumnsI[i], 2))
			{
				return true;
			}
		}
		return false;
	}

	private bool HiddenTuple_Find(Region region, int amount)
	{
		// If there are only "amount" cells with candidates, we don't have to waste our time
		if (region.CountCellsWithCandidates() == amount)
		{
			return false;
		}

		return HiddenTuple_Recurse(region, amount, 0, new int[amount]);
	}
	private bool HiddenTuple_Recurse(Region region, int amount, int loop, int[] candidates)
	{
		if (loop == amount)
		{
			IEnumerable<Cell> cells = candidates.Select(region.GetCellsWithCandidate).UniteAll();
			IEnumerable<int> cands = cells.Select(c => (IEnumerable<int>)c.CandI).UniteAll();

			if (cells.Count() != amount // There aren't "amount" cells for our tuple to be in
				|| cands.Count() == amount // We already know it's a tuple (might be faster to skip this check, idk)
				|| !cands.ContainsAll(candidates))
			{
				return false; // If a number in our combo doesn't actually show up in any of our cells
			}

			if (Cell.ChangeCandidates(cells, Utils.OneToNine.Except(candidates)))
			{
				LogAction(TechniqueFormat("Hidden " + TupleStr[amount],
					"{0}: {1}",
					cells.Print(), Utils.PrintCandidates(candidates)),
					cells);
				return true;
			}
		}
		else
		{
			for (int i = candidates[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
			{
				candidates[loop] = i;
				if (HiddenTuple_Recurse(region, amount, loop + 1, candidates))
				{
					return true;
				}
			}
		}
		return false;
	}
}
