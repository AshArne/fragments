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

namespace Gyro_L1
{
    public sealed class Program : MyGridProgram
    {
        #region Copy start

        // Объявляем класс управления гироскопами
        private readonly GyroClass Gyro;

        public Program()
        {
            // Создаем новый экземпляр класса, передав в него ссылку на
            // Программируемый блок, внутри которого будет выполняться скрипт.
            Gyro = new GyroClass(this);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.ToUpper())
            {
                case "PITCH":
                    // Передаем управление гироскопами скрипту
                    Gyro.Override(true);
                    // Запускаем вращение по оси тангажа со скоростью 5 RPM
                    Gyro.SetPitchRPM(1);
                    break;
                case "ROLL":
                    // Передаем управление гироскопами скрипту
                    Gyro.Override(true);
                    // Запускаем вращение по оси крена со скоростью 5 RPM
                    Gyro.SetRollRPM(1);
                    break;
                case "YAW":
                    // Передаем управление гироскопами скрипту
                    Gyro.Override(true);
                    // Запускаем вращение по оси рыскания со скоростью 5 RPM
                    Gyro.SetYawRPM(1);
                    break;
                case "STOP":
                    // Забираем у скрипта возможность управления гироскопами
                    Gyro.Override(false);
                    break;
            }
        }

        /// <summary>
        /// Класс для управления гироскопами корабля
        /// </summary>
        internal class GyroClass
        {
            private readonly List<IMyGyro> Gyros = new List<IMyGyro>();

            /// <summary>
            /// Инициализатор класса
            /// </summary>
            /// <param name="program">Ссылка на класс, описывающий Программируемый блок</param>
            public GyroClass(Program prog)
            {
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
            internal void SetPitchRPM(float pitch)
            {
                foreach (var gyro in Gyros)
                {
                    gyro.Pitch = pitch;
                }
            }

            /// <summary>
            /// Установить RPM по оси рыскания
            /// </summary>
            /// <param name="yaw">Значение RPM</param>
            internal void SetYawRPM(float yaw)
            {
                foreach (var gyro in Gyros)
                {
                    gyro.Yaw = yaw;
                }
            }

            /// <summary>
            /// Установить RPM по оси крена
            /// </summary>
            /// <param name="roll">Значение RPM</param>
            internal void SetRollRPM(float roll)
            {
                foreach (var gyro in Gyros)
                {
                    gyro.Roll = roll;
                }
            }
        }

        #endregion
    }
}
