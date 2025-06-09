﻿using Elsa.Common.Entities;
using Elsa.Common.Models;
using Elsa.Extensions;
using Elsa.Persistence.Dapper.Extensions;
using Elsa.Persistence.Dapper.Models;
using Elsa.Persistence.Dapper.Modules.Runtime.Records;
using Elsa.Persistence.Dapper.Services;
using Elsa.Workflows;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Entities;
using Elsa.Workflows.Runtime.Filters;
using Elsa.Workflows.Runtime.OrderDefinitions;
using JetBrains.Annotations;

namespace Elsa.Persistence.Dapper.Modules.Runtime.Stores;

/// <summary>
/// Provides a Dapper implementation of <see cref="ITriggerStore"/>.
/// </summary>
[UsedImplicitly]
internal class DapperTriggerStore(Store<StoredTriggerRecord> store, IPayloadSerializer payloadSerializer) : ITriggerStore
{
    /// <inheritdoc />
    public async ValueTask SaveAsync(StoredTrigger record, CancellationToken cancellationToken = default)
    {
        var mappedRecord = Map(record);
        await store.SaveAsync(mappedRecord, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask SaveManyAsync(IEnumerable<StoredTrigger> records, CancellationToken cancellationToken = default)
    {
        var mappedRecords = records.Select(Map);
        await store.SaveManyAsync(mappedRecords, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<StoredTrigger?> FindAsync(TriggerFilter filter, CancellationToken cancellationToken = default)
    {
        var record = await store.FindAsync(q => ApplyFilter(q, filter), cancellationToken);
        return record != null ? Map(record) : null;
    }

    /// <inheritdoc />
    public async ValueTask<IEnumerable<StoredTrigger>> FindManyAsync(TriggerFilter filter, CancellationToken cancellationToken = default)
    {
        var records = await store.FindManyAsync(q => ApplyFilter(q, filter), cancellationToken);
        return Map(records);
    }

    public ValueTask<Page<StoredTrigger>> FindManyAsync(TriggerFilter filter, PageArgs pageArgs, CancellationToken cancellationToken = default)
    {
        return FindManyAsync(filter, pageArgs, new StoredTriggerOrder<string>(x => x.Id, OrderDirection.Ascending), cancellationToken);
    }

    public async ValueTask<Page<StoredTrigger>> FindManyAsync<TProp>(TriggerFilter filter, PageArgs pageArgs, StoredTriggerOrder<TProp> order, CancellationToken cancellationToken = default)
    {
        var page = await store.FindManyAsync(q => ApplyFilter(q, filter), pageArgs, order.KeySelector.GetPropertyName(), order.Direction, cancellationToken);
        return Map(page);
    }

    /// <inheritdoc />
    public async ValueTask ReplaceAsync(IEnumerable<StoredTrigger> removed, IEnumerable<StoredTrigger> added, CancellationToken cancellationToken = default)
    {
        var filter = new TriggerFilter
        {
            Ids = removed.Select(r => r.Id).ToList()
        };
        await DeleteManyAsync(filter, cancellationToken);
        await SaveManyAsync(added, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<long> DeleteManyAsync(TriggerFilter filter, CancellationToken cancellationToken = default)
    {
        return await store.DeleteAsync(q => ApplyFilter(q, filter), cancellationToken);
    }

    private void ApplyFilter(ParameterizedQuery query, TriggerFilter filter)
    {
        query
            .Is(nameof(StoredTriggerRecord.Id), filter.Id)
            .In(nameof(StoredTriggerRecord.Id), filter.Ids)
            .Is(nameof(StoredTriggerRecord.WorkflowDefinitionId), filter.WorkflowDefinitionId)
            .In(nameof(StoredTriggerRecord.WorkflowDefinitionId), filter.WorkflowDefinitionIds)
            .Is(nameof(StoredTriggerRecord.WorkflowDefinitionVersionId), filter.WorkflowDefinitionVersionId)
            .In(nameof(StoredTriggerRecord.WorkflowDefinitionVersionId), filter.WorkflowDefinitionVersionIds)
            .Is(nameof(StoredTriggerRecord.Name), filter.Name)
            .In(nameof(StoredTriggerRecord.Name), filter.Names)
            .Is(nameof(StoredTriggerRecord.Hash), filter.Hash)
            ;
    }
    
    private Page<StoredTrigger> Map(Page<StoredTriggerRecord> source)
    {
        return new(source.Items.Select(Map).ToList(), source.TotalCount);
    }

    private IEnumerable<StoredTrigger> Map(IEnumerable<StoredTriggerRecord> source) => source.Select(Map);

    private StoredTrigger Map(StoredTriggerRecord source)
    {
        return new StoredTrigger
        {
            Id = source.Id,
            ActivityId = source.ActivityId,
            Hash = source.Hash,
            Name = source.Name,
            WorkflowDefinitionId = source.WorkflowDefinitionId,
            WorkflowDefinitionVersionId = source.WorkflowDefinitionVersionId,
            Payload = source.SerializedPayload != null ? payloadSerializer.Deserialize(source.SerializedPayload) : null,
            TenantId = source.TenantId
        };
    }

    private StoredTriggerRecord Map(StoredTrigger source)
    {
        return new StoredTriggerRecord
        {
            Id = source.Id,
            ActivityId = source.ActivityId,
            Hash = source.Hash,
            Name = source.Name,
            WorkflowDefinitionId = source.WorkflowDefinitionId,
            WorkflowDefinitionVersionId = source.WorkflowDefinitionVersionId,
            SerializedPayload = source.Payload != null ? payloadSerializer.Serialize(source.Payload) : null,
            TenantId = source.TenantId
        };
    }
}