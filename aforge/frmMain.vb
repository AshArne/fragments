Public Class frmMain
' Коллекция устройств видеозахвата
Private VideoDevices As AForge.Video.DirectShow.FilterInfoCollection
' Создание коллекции устройств видеозахвата
Private Sub butRefresh_Click(ByVal sender As System.Object,
	ByVal e As System.EventArgs) Handles butRefresh.Click

	VideoDevices = New AForge.Video.DirectShow.FilterInfoCollection
		(AForge.Video.DirectShow.FilterCategory.VideoInputDevice)
	' Очистка списка устройств
	cmbCameras.Items.Clear()
	' Если устройства не найдены
	If VideoDevices.Count = 0 Then
		cmbCameras.Text = "Not found"
	Else
		' Если устройства найдены, добавить их имена в список
		For Each Device As AForge.Video.DirectShow.FilterInfo In VideoDevices
			cmbCameras.Items.Add(Device.Name)
		Next
		' Выбрать первое устройство из списка
		cmbCameras.SelectedIndex = 0
	End If
End Sub
End Class
