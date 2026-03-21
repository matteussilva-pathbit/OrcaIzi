﻿namespace OrcaIzi.Domain.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateBudgetPdfAsync(Budget budget);
    }
}



