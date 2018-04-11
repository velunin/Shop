using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using NSaga;

namespace Shop.DataAccess.EF.NSaga
{
    // ReSharper disable once InconsistentNaming
    public class NSagaEFRepository : ISagaRepository
    {
        private readonly ShopDbContext _dbContext;
        private readonly ISagaFactory _sagaFactory;
        private readonly IMessageSerialiser _messageSerialiser;

        public NSagaEFRepository(ShopDbContext dbContext, ISagaFactory sagaFactory, IMessageSerialiser messageSerialiser)
        {
            _dbContext = dbContext;
            _sagaFactory = sagaFactory;
            _messageSerialiser = messageSerialiser;
        }

        public TSaga Find<TSaga>(Guid correlationId) where TSaga : class, IAccessibleSaga
        {
            var persistedData = _dbContext.Sagas.SingleOrDefault(s => s.CorrelationId == correlationId);
            if (persistedData == null)
            {
                return null;
            }

            var sagaInstance = _sagaFactory.ResolveSaga<TSaga>();
            var sagaDataType = GetInterfaceGenericType<TSaga>(typeof(ISaga<>));
            var sagaData = _messageSerialiser.Deserialise(persistedData.Data, sagaDataType);

            var headersPersisted = _dbContext.SagaHeaders.Where(h => h.CorrelationId == correlationId);
            var headers = headersPersisted.ToDictionary(k => k.Key, v => v.Value);

            sagaInstance.CorrelationId = correlationId;
            sagaInstance.Headers = headers;

            SetSagaData(sagaInstance, sagaData);

            return sagaInstance;
        }

        public void Save<TSaga>(TSaga saga) where TSaga : class, IAccessibleSaga
        {
            var sagaData = GetSagaData(saga);
            var sagaHeaders = saga.Headers;
            var correlationId = saga.CorrelationId;

            var serialisedData = _messageSerialiser.Serialise(sagaData);

            var dataModel = new SagaData
            {
                CorrelationId = correlationId,
                Data = serialisedData,
            };

            try
            {
                _dbContext.Database.BeginTransaction();

                _dbContext.Attach(dataModel);

                var updatedRows = _dbContext.SaveChanges();

                if (updatedRows == 0)
                {
                    _dbContext.Add(dataModel);
                }

                var headers = _dbContext.SagaHeaders
                    .Where(x => x.CorrelationId == correlationId)
                    .AsEnumerable();

                _dbContext.SagaHeaders.RemoveRange(headers);

                foreach (var header in sagaHeaders)
                {
                    var storedHeader = new SagaHeaders
                    {
                        CorrelationId = correlationId,
                        Key = header.Key,
                        Value = header.Value,
                    };

                    _dbContext.Add(storedHeader);
                }

                _dbContext.SaveChanges();

                _dbContext.Database.CommitTransaction();
            }
            catch (Exception)
            {
                _dbContext.Database.RollbackTransaction();
                throw;
            }
        }

        public void Complete<TSaga>(TSaga saga) where TSaga : class, IAccessibleSaga
        {
            var correlationId = GetCorrelationId(saga);

            Complete(correlationId);
        }

        public void Complete(Guid correlationId)
        {
            var saga = _dbContext.Sagas.Single(x => x.CorrelationId == correlationId);
            var headers = _dbContext.SagaHeaders
                .Where(x => x.CorrelationId == correlationId)
                .AsEnumerable();

            try
            {
                _dbContext.Database.BeginTransaction();

                _dbContext.Remove(saga);
                _dbContext.RemoveRange(headers);
                _dbContext.SaveChanges();

                _dbContext.Database.CommitTransaction();
            }
            catch (Exception)
            {
                _dbContext.Database.RollbackTransaction();
                throw;
            }
        }

        private static object GetSagaData<TSaga>(TSaga saga) where TSaga : class, IAccessibleSaga
        {
            var property = saga.GetType().GetProperty("SagaData");

            return property?.GetValue(saga, null);
        }

        private static void SetSagaData<TSaga>(TSaga saga, object sagaData) where TSaga : class, IAccessibleSaga
        {
            var property = saga.GetType().GetProperty("SagaData");
            if (property != null && property.CanWrite)
            {
                property.SetValue(saga, sagaData, null);
            }
        }

        private static Guid GetCorrelationId<TSaga>(TSaga saga) where TSaga : class, IAccessibleSaga
        {
            var property = saga.GetType().GetProperty("CorrelationId");

            if (property == null)
            {
                throw new InvalidOperationException();
            }

            return (Guid) property.GetValue(saga, null);
        }

        private static Type GetInterfaceGenericType<TInstance>(MemberInfo interfaceType)
        {
            var instanceType = typeof(TInstance);
            return GetInterfaceGenericType(interfaceType, instanceType);
        }

        private static Type GetInterfaceGenericType(MemberInfo interfaceType, Type instanceType)
        {
            var type = instanceType.GetInterface(interfaceType.Name);

            return type.IsGenericType 
                ? type.GetGenericArguments().FirstOrDefault()
                : null;
        }
    }

    [Table("NSaga.Sagas")]
    public class SagaData
    {
        [Key]
        public Guid CorrelationId { get; set; }

        public string Data { get; set; }
    }

    [Table("NSaga.Headers")]
    public class SagaHeaders
    {
        [Key]
        public Guid CorrelationId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}