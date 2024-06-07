﻿namespace PhotoManager.Domain.Interfaces;

public interface ISyncAssetsService
{
    Task<List<SyncAssetsResult>> ExecuteAsync(ProcessStatusChangedCallback callback);
}