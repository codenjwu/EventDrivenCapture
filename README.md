# EventDrivenCapture
An Event Driven Screen Capturing Library

An event driven, continuous screen captruing library that support multi screen area capturing. Support start, pause, continue, stop operation.

## Easy to use!
```csharp
var handler = new EventDrivenCapture.CaptureEventHandler(new EventDrivenCapture.Capture[1] { new Capture(new CaptureSetting(100,100,100,100)) }); // init handler, require screen area settings
handler.CapturedEventHandler += captured; // register your captured event handler, like refreshing the screen
handler.ExceptionEventHandler += exception; // register your exception handler, 
handler.CanceledEventHandler += canceled; // register your capture task cancel handler
handler.Start();

private void canceled(object sender, CaptureEventArgs[] e)
{
    MessageBox.Show("job is canceled");
}

private void exception(object sender, CaptureEventArgs[] e)
{
    MessageBox.Show($"exception raised");
    handler.Stop();
}

private void captured(object sender, CaptureEventArgs[] e) // this could throw exception, for testing ExceptionEventHandler purpose
{
    this.pictureBox1.Image = e[0]?.Capture?.CapturedImage;
    this.pictureBox2.Image = e[1]?.Capture?.CapturedImage;
}
```
### Demo
Demo is provided.
