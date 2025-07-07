using System.Collections.Generic;

namespace Hermes.Parking.Server.DataService
{
    /// <summary>
    /// Задача этого сервиса - хранение данных. Сервис делегирует работу с объектами соответствующим провайдерам по их именам.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Список поддерживаемых типов объектов и обрабатывающих их провайдеров данных.
        /// </summary>
        IDictionary<string, IList<IDataProvider>> ObjectTypes { get; }

        /// <summary>
        /// Создает объект на основе начальных параметров InitialData.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="InitialData">Начальные данные для создания объекта. Конкретный набор данных зависит от провайдера.</param>        
        T Create<T>(string ObjectType, object InitialObject);

        /// <summary>
        /// Возвращает объект по фильтру.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="InitialData">Фильтр для поиска объекта. Конкретный набор данных зависит от провайдера.</param>
        T Get<T>(string ObjectType, IDictionary<string, object> Filter);

        /// <summary>
        /// Возвращает объект по его ID.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="Id">Идентификатор объекта.</param>
        T Get<T>(string ObjectType, long Id);

        /// <summary>
        /// Возвращает список объектов по фильтру.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="InitialData">Фильтр для поиска объектов. Конкретный набор данных зависит от провайдера.</param>
        IEnumerable<T> GetList<T>(string ObjectType, IDictionary<string, object> Filter);

        /// <summary>
        /// Сохраняет объект.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="Object">Объект для сохранения.</param>
        void Save(string ObjectType, object Object);

        /// <summary>
        /// Удаляет объект из хранилища.
        /// </summary>
        /// <param name="ObjectType">Тип объекта.</param>
        /// <param name="Object">Объект для удаления.</param>
        void Delete(string ObjectType, object Object);
    }
}
