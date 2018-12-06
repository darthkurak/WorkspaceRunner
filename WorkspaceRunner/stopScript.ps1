Start-Job -ScriptBlock {az vm stop -g Lingaro_IoTPlatform_kuba_dev -n jdrotester}
Start-Job -ScriptBlock {az vm stop -g Lingaro_IoTPlatform_kuba_dev -n jdroorleans}
Start-Job -ScriptBlock {az eventhubs namespace update --name lingarojdro --resource-group Lingaro_IoTPlatform_kuba_dev --capacity 1}
Get-Job | Wait-Job | Receive-Job