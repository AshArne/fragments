using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace Gyro_L2
{
    public sealed class Program : MyGridProgram
    {
        #region Copy start

        // Константа, хранящая имя кокпита
        private const string SHIP_CONTROLLER_NAME = "Кокпит истребителя";
        // Коэффициент чувствительности по оси крена
        private const float GYRO_ROLL_SENSITIVITY = 5;
        // Коэффициент чувствительности по оси тангажа
        private const float GYRO_PITCH_SENSITIVITY = 5;
        // Коэффициент чувствительности по оси рыскания
        private const float GYRO_YAW_SENSITIVITY = 0.5f;

        // Объявляем класс управления гироскопами
        private readonly GyroClass Gyro;

        // Конструктор
        public Program()
        {
            // Получаем ссылку на кокпит, который далее будет выполнять роль контроллера корабля
            IMyShipController ship_controller = GridTerminalSystem.GetBlockWithName(SHIP_CONTROLLER_NAME) as IMyShipController;
            // Создаем новый экземпляр класса GyroClass, передав в него ссылку на
            // Программируемый блок, внутри которого будет выполняться скрипт.
            // Сразу же устанавливаем свойствам класса значения констант.
            Gyro = new GyroClass(this, ref ship_controller)
            {
                RollSensitivity = GYRO_ROLL_SENSITIVITY,
                PitchSensitivity = GYRO_PITCH_SENSITIVITY,
                YawSensitivity = GYRO_YAW_SENSITIVITY
            };
            /*
             * Код, написанный выше, аналогичен закомментированным строкам ниже
            Gyro = new GyroClass(this, ref ship_controller);
            Gyro.RollSensitivity = GYRO_ROLL_SENSITIVITY;
            Gyro.PitchSensitivity = GYRO_PITCH_SENSITIVITY;
            Gyro.YawSensitivity = GYRO_YAW_SENSITIVITY;
            */
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                // Обработка вызова скрипта из терминала или по нажатию на кнопку в кокпите
                case UpdateType.Trigger:
                case UpdateType.Terminal:
                    switch (argument.ToUpper())
                    {
                        case "START":
                            // Передаем управление гироскопами скрипту
                            Gyro.Override(true);
                            // Запускаем скрипт в режим "каждотикового" выполнения
                            Runtime.UpdateFrequency = UpdateFrequency.Update1;
                            break;
                        case "STOP":
                            // Забираем у скрипта возможность управления гироскопами
                            Gyro.Override(false);
                            // Останавливаем "тикающее" выполнение скрипта для
                            // экономии ресурсов компьютера
                            Runtime.UpdateFrequency = UpdateFrequency.None;
                            break;
                    }
                    break;

                // Каждый тик вызываем метод GravityAligment(),
                // выравнивающий корабль во вектору гравитации.
                case UpdateType.Update1:
                    Gyro.GravityAligment();
                    break;
            }
        }

        /// <summary>
        /// Класс для управления гироскопами корабля
        /// </summary>
        internal class GyroClass
        {
            // Переменная, хранящая ссылку на контроллер корабля (в нашем случае кокпит)
            private readonly IMyShipController SControl;
            // Создаем новый список для хранения гироскопов
            private readonly List<IMyGyro> Gyros = new List<IMyGyro>();

            /// <summary>
            /// Свойство: коэффициент чувствительности по оси тангажа
            /// </summary>
            internal float PitchSensitivity { get; set; } = 1;

            /// <summary>
            /// Свойство: коэффициент чувствительности по оси крена
            /// </summary>
            internal float RollSensitivity { get; set; } = 1;

            /// <summary>
            /// Свойство: коэффициент чувствительности по оси рыскания
            /// </summary>
            internal float YawSensitivity { get; set; } = 1;

            /// <summary>
            /// Конструктор класса
            /// </summary>
            /// <param name="program">Ссылка на класс, описывающий Программируемый блок</param>
            /// <param name="shipController">Ссылка на класс, описывающий контроллер корабля</param>
            public GyroClass(Program prog, ref IMyShipController shipController)
            {
                // Запоминаем ссылку на контроллер корабля
                SControl = shipController;
                // Добавляем в список Gyros все гироскопы, но только установленные на корабле.
                // Гироскопы пристыкованных гридов отфильтруются лямбда-выражением и не добавятся.
                prog.GridTerminalSystem.GetBlocksOfType(Gyros, filter => filter.CubeGrid == prog.Me.CubeGrid);
            }

            /// <summary>
            /// Функция перехвата управления скриптом. Отключает игрока от управления.
            /// </summary>
            /// <param name="is_script_controlling">Если True, то управляет гироскопами скрипт.
            /// False - игрок.</param>
            internal void Override(bool is_script_controlling)
            {
                // Перебираем в цикле все гироскопы
                foreach (var gyro in Gyros)
                {
                    // Устанавливаем свойство "Перехват".
                    gyro.GyroOverride = is_script_controlling;
                    // Обнуляем все крутящие моменты
                    gyro.Roll = 0;      // Крен
                    gyro.Yaw = 0;       // Рысканье
                    gyro.Pitch = 0;     // Тангаж
                }
            }

            /// <summary>
            /// Установить RPM по оси тангажа
            /// </summary>
            /// <param name="pitch">Значение RPM</param>
            private void SetPitchRPM(float pitch)
            {
                foreach (var gyro in Gyros)
                    gyro.Pitch = pitch;
            }

            /// <summary>
            /// Установить RPM по оси рыскания
            /// </summary>
            /// <param name="yaw">Значение RPM</param>
            private void SetYawRPM(float yaw)
            {
                foreach (var gyro in Gyros)
                    gyro.Yaw = yaw;
            }

            /// <summary>
            /// Установить RPM по оси крена
            /// </summary>
            /// <param name="roll">Значение RPM</param>
            private void SetRollRPM(float roll)
            {
                foreach (var gyro in Gyros)
                    gyro.Roll = roll;
            }

            /// <summary>
            /// Метод: выравнивание корабля перпендикулярно вектору гравитации планеты
            /// </summary>
            internal void GravityAligment()
            {
                // Получаем от контроллера корабля вектор гравитации и нормируем его
                Vector3D gravity = Vector3D.Normalize(SControl.GetNaturalGravity());

                // Скалярным произведением (Dot) рассчитываем угол между нормированным вектором гравитации
                // и вектором корабля, указывающим назад в координатах матрицы мира игры.
                // Преобразуем его к типу данных float.
                // Умножаем на коэффициент чувствительности, получив тем самым величину вращающего момента.
                float pitch = (float)gravity.Dot(SControl.WorldMatrix.Backward) * PitchSensitivity;
                // Передаем на все гироскопы величину вращающего момента в RPM вдоль оси тангажа.
                SetPitchRPM(pitch);

                // Аналогично для оси крена.
                float roll = (float)gravity.Dot(SControl.WorldMatrix.Left) * RollSensitivity;
                SetRollRPM(roll);

                // Получаем от контроллера корабля значение индикатора вращения по оси рыскания.
                // Умножаем его на коэффициент чувствительности, получая таким образом величину
                // вращающего момента.
                float yaw = SControl.RotationIndicator.Y * YawSensitivity;
                // Передаем вращающий момент на гироскопы.
                SetYawRPM(yaw);
            }
        }

        #endregion
    }
}
