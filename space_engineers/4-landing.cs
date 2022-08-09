/*
Скрипт статьи "Space Engineers - скрипт управления посадочными шасси"
https://zen.yandex.ru/media/unpromresdept/space-engineers-skript-upravleniia-posadochnymi-shassi-62f260e5e2204007272e8543
*/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace Template
{
    public sealed class Program : MyGridProgram
    {
        #region Copy

        // Константа, задающая скорость движения поршня
        private const float PISTON_VELOCITY = 1;

        // Структура для хранения пары поршень-шасси. В дальнейшем такая пара будет называться "опора"
        private struct LandingSupport
        {
            // Ссылка на блок поршень
            public IMyPistonBase Piston;
            // Ссылка на блок шасси
            public IMyLandingGear LandingGear;
        }

        // Создаем новый список опор
        private readonly List<LandingSupport> Supports = new List<LandingSupport>();
        // Счетчик опор, прикрепившихся своими шасси к поверхности
        private int SupportsReadyCounter;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;

            // Добавляем в список новую опору, состоящую из пары поршень-шасси
            Supports.Add(new LandingSupport() {
                Piston = GridTerminalSystem.GetBlockWithName("Поршень 1") as IMyPistonBase,
                LandingGear = GridTerminalSystem.GetBlockWithName("Посадочные шасси 1") as IMyLandingGear
            });
            // Добавляем в список вторую опору
            Supports.Add(new LandingSupport()
            {
                Piston = GridTerminalSystem.GetBlockWithName("Поршень 2") as IMyPistonBase,
                LandingGear = GridTerminalSystem.GetBlockWithName("Посадочные шасси 2") as IMyLandingGear
            });
            // Третью
            Supports.Add(new LandingSupport()
            {
                Piston = GridTerminalSystem.GetBlockWithName("Поршень 3") as IMyPistonBase,
                LandingGear = GridTerminalSystem.GetBlockWithName("Посадочные шасси 3") as IMyLandingGear
            });
            // Четвертую
            Supports.Add(new LandingSupport()
            {
                Piston = GridTerminalSystem.GetBlockWithName("Поршень 4") as IMyPistonBase,
                LandingGear = GridTerminalSystem.GetBlockWithName("Посадочные шасси 4") as IMyLandingGear
            });

            // Перебираем в цикле все опоры из списка
            for (int i = 0; i < Supports.Count; i++)
            {
                // Устанавливаем максимально возможную дистанцию выдвижения поршня опоры на 10 м
                Supports[i].Piston.MaxLimit = 10;
                // Устанавливаем минимальную дистанцию для сложенного поршня, 0 м
                Supports[i].Piston.MinLimit = 0;
                // Принудительно втягиваем все поршни, установив им отрицательную скорость
                Supports[i].Piston.Velocity = -PISTON_VELOCITY;
                // Отключаем у шасси автозацеп
                Supports[i].LandingGear.AutoLock = false;
                // Отключаем у шасси режим парковки по нажатии игроком кнопки "Р"
                Supports[i].LandingGear.IsParkingEnabled = false;

                // Выводим в терминале название поршня, входящего в опору
                Echo(Supports[i].Piston.DisplayNameText);
                // Выводим в терминале название шасси, входящего в опору
                Echo(Supports[i].LandingGear.DisplayNameText);
            }
        }
        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                // Блок обработки нажатия игроком на кнопку кокпита или кнопку "Выполнить", находящуюся в терминале,
                // в свойствах Программируемого блока
                case UpdateType.Trigger:
                case UpdateType.Terminal:
                    switch (argument.ToUpper())
                    {
                        // Блок обработки аргумента "DOWN" - выдвижение опоры
                        case "DOWN":
                            // Обнуляем счетчик опор, примагнитившихся к поверхности
                            SupportsReadyCounter = 0;
                            // Перебираем все опоры в цикле
                            foreach (var support in Supports)
                            {
                                // Для поршня, входящего в опору, устанавливаем положительную скорость, выдвигая его
                                support.Piston.Velocity = PISTON_VELOCITY;
                            }
                            // Включаем вызов метода Main() каждый тик
                            Runtime.UpdateFrequency = UpdateFrequency.Update1;
                            break;

                        // Блок обработки аргумента "UP" - поднятие опоры
                        case "UP":
                            // Перебираем все опоры в цикле
                            foreach (var support in Supports)
                            {
                                // Открепляем магнитное шасси от поверхноси
                                support.LandingGear.Unlock();
                                // Для поршня, входящего в опору, устанавливаем отрицательную скорость, задвигая его
                                support.Piston.Velocity = -PISTON_VELOCITY;
                            }
                            break;
                    }
                    break;

                // Блок обработки вызова метода Main(), происходящий каждый тик
                case UpdateType.Update1:
                    // Перебираем все опоры в цикле
                    foreach (var support in Supports)
                    {
                        // Проверяем, готово ли шасси, входящее в опору, к прикреплению к земле
                        if (support.LandingGear.LockMode == LandingGearMode.ReadyToLock)
                        {
                            // Если готово, то останавливаем поршень, входящий в эту же опору
                            support.Piston.Velocity = 0;
                            // Примагничиваем шасси
                            support.LandingGear.Lock();
                            // Инкрементируем (увеличиваем) на единицу счетчик опор, закончивших выдвижение
                            SupportsReadyCounter++;
                        }
                    }
                    // Если количество закончивших выдвижение опор равно общему количеству опор,
                    // останавливаем работу скрипта
                    if (SupportsReadyCounter == Supports.Count)
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                    break;
            }
        }

        #endregion
    }
}
