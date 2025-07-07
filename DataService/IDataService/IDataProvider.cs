using System.Collections.Generic;

namespace Hermes.Parking.Server.DataService
{
    /// <summary>
    /// Провайдер обеспечевиет хранение данных определенного вида.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// По имени провайдера происходит его выбор при обработке данных конкретного вида.
        /// </summary>
        string GetName();

        /// <summary>
        /// Создает объект на основе начальных параметров InitialData.
        /// </summary>
        /// <param name="InitialData">Начальные данные для создания объекта. Конкретный набор данных зависит от самого провайдера.</param>
        void Create(string ObjectType, object InitialObject, ref object Output);

        /// <summary>
        /// Получает объект по фильтру.
        /// </summary>
        /// <param name="InitialData">Фильтр для поиска объекта. Конкретный набор данных зависит от самого провайдера.</param>
        void Get(string ObjectType, IDictionary<string, object> Filter, ref object Output);

        /// <summary>
        /// Получает объект по Id.
        /// </summary>
        void Get(string ObjectType, long Id, ref object Output);

        /// <summary>
        /// Получает список объектов по фильтру.
        /// </summary>
        /// <param name="InitialData">Фильтр для поиска объектов. Конкретный набор данных зависит от самого провайдера.</param>
        void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output);

        /// <summary>
        /// Удаляет конкретный объект.
        /// </summary>
        /// <param name="Object">Объект для удаления.</param>
        void Delete(string ObjectType, object Object);

        /// <summary>
        /// Сохраняет объект.
        /// </summary>
        /// <param name="Object">Объект для сохранения.</param>
        void Save(string ObjectType, object Object);
    }
}