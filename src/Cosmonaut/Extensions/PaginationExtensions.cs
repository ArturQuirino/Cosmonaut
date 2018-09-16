﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Extensions
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> WithPagination<T>(this IQueryable<T> queryable, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be a positive number.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            return GetQueryableWithPaginationSettings(queryable, $"{nameof(WithPagination)}/{pageNumber}", pageSize);
        }

        public static IQueryable<T> WithPagination<T>(this IQueryable<T> queryable, string continuationToken, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be a positive number.");
            }

            return GetQueryableWithPaginationSettings(queryable, continuationToken, pageSize);
        }

        private static IQueryable<T> GetQueryableWithPaginationSettings<T>(IQueryable<T> queryable, string continuationInfo, int pageSize)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return queryable;

            var feedOptions = queryable.GetFeedOptionsForQueryable();

            if (feedOptions == null) return queryable;

            feedOptions.MaxItemCount = pageSize;
            feedOptions.RequestContinuation = continuationInfo;
            queryable.SetFeedOptionsForQueryable(feedOptions);
            return queryable;
        }

        internal static FeedOptions GetFeedOptionsForQueryable<T>(this IQueryable<T> queryable)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return null;

            return (FeedOptions)queryable.Provider.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(queryable.Provider);
        }

        internal static void SetFeedOptionsForQueryable<T>(this IQueryable<T> queryable, FeedOptions feedOptions)
        {
            if (!queryable.GetType().Name.Equals("DocumentQuery`1"))
                return;

            queryable.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(queryable, feedOptions);
            queryable.Provider.GetType().GetTypeInfo().GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(queryable.Provider, feedOptions);
        }
    }
}