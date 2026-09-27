// ============================================================
// WardView.cs
// ------------------------------------------------------------
// Draws the hospital in a SplashKit window (menu option 8).
//   - each ward is a row of coloured boxes, one box per bed
//   - green = free, red = occupied, orange = patient needs a different ward
//   - the waiting list is shown on the right
//   - clicking a bed shows that patient's details at the bottom
//
// This class only READS from the Hospital. It never changes anything,
// so all the rules stay in Hospital and the wards.
//
// It works like the game loop in RobotDodge: every frame we process
// events (mouse clicks), handle input, redraw everything, then refresh.
// The loop ends when the user closes the window, and control goes
// back to the console menu in Program.cs.
// ============================================================

using System.Collections.Generic;
using SplashKitSDK;

public class WardView
{
    // Layout sizes in pixels, kept in one place so they are easy to adjust.
    private const int BED_WIDTH = 85;
    private const int BED_HEIGHT = 60;
    private const int BED_GAP = 10;      // space between two beds
    private const int ROW_HEIGHT = 105;  // vertical space for each ward
    private const int TOP = 45;          // y position of the first ward
    private const int LEFT = 20;         // left margin

    private Hospital _hospital;
    private Bed _selectedBed;    // the bed the user last clicked, or null
    private Ward _selectedWard;  // the ward that bed belongs to

    public WardView(Hospital hospital)
    {
        _hospital = hospital;
    }

    // Opens the window and keeps it running until the user closes it.
    public void Show()
    {
        // Start with nothing selected each time the map is opened.
        _selectedBed = null;
        _selectedWard = null;

        // The window grows with the number of wards (changed in C1, when a
        // fifth ward made the old fixed size draw beds over the legend).
        int height = (int)GetDetailsY() + 95;
        Window window = new Window("Patient Management - Ward Map", 820, height);

        while (!window.CloseRequested)
        {
            SplashKit.ProcessEvents();   // must be called every frame to read mouse and keyboard

            if (SplashKit.MouseClicked(MouseButton.LeftButton))
                SelectBedAt(SplashKit.MousePosition());

            Draw(window);
            window.Refresh(60);          // show the new frame, at most 60 frames per second
        }

        window.Close();
    }

    // Works out the rectangle for one bed from its ward row and bed position.
    // Drawing AND clicking both use this method, so the area you click
    // always matches exactly what is drawn on screen.
    private Rectangle GetBedArea(int wardIndex, int bedIndex)
    {
        double x = LEFT + bedIndex * (BED_WIDTH + BED_GAP);
        double y = TOP + wardIndex * ROW_HEIGHT + 18;   // +18 leaves room for the ward title
        return SplashKit.RectangleFrom(x, y, BED_WIDTH, BED_HEIGHT);
    }

    // The legend and the details box sit just below the last ward row,
    // so adding a ward moves them down instead of drawing over them.
    private double GetLegendY()
    {
        return TOP + _hospital.GetWards().Count * ROW_HEIGHT + 5;
    }

    private double GetDetailsY()
    {
        return GetLegendY() + 35;
    }

    // Checks every bed to see if the mouse click landed inside it.
    private void SelectBedAt(Point2D point)
    {
        List<Ward> wards = _hospital.GetWards();

        for (int w = 0; w < wards.Count; w++)
        {
            for (int b = 0; b < wards[w].Beds.Count; b++)
            {
                if (SplashKit.PointInRectangle(point, GetBedArea(w, b)))
                {
                    _selectedWard = wards[w];
                    _selectedBed = wards[w].Beds[b];
                    return;   // found it, no need to check the rest
                }
            }
        }
        // Clicking empty space keeps the current selection.
    }

    // Redraws the whole screen. Called 60 times a second, so any change
    // in the hospital shows up straight away.
    private void Draw(Window window)
    {
        window.Clear(Color.White);
        window.DrawText("Patient Management - Ward Map", Color.Black, LEFT, 15);

        DrawWards(window);
        DrawWaitingList(window);
        DrawLegend(window);
        DrawDetails(window);
    }

    private void DrawWards(Window window)
    {
        List<Ward> wards = _hospital.GetWards();

        for (int w = 0; w < wards.Count; w++)
        {
            Ward ward = wards[w];
            double rowY = TOP + w * ROW_HEIGHT;

            // GetAdmissionRule picks the right text from the ward type.
            window.DrawText($"{ward.Name} ({ward.GetAdmissionRule()})", Color.Black, LEFT, rowY);

            for (int b = 0; b < ward.Beds.Count; b++)
            {
                Bed bed = ward.Beds[b];
                Rectangle area = GetBedArea(w, b);

                // Coloured box with a black border.
                window.FillRectangle(GetBedColour(ward, bed), area.X, area.Y, area.Width, area.Height);
                window.DrawRectangle(Color.Black, area.X, area.Y, area.Width, area.Height);

                // A thicker blue outline around the selected bed
                // (two rectangles, one pixel apart).
                if (bed == _selectedBed)
                {
                    window.DrawRectangle(Color.Blue, area.X - 2, area.Y - 2, area.Width + 4, area.Height + 4);
                    window.DrawRectangle(Color.Blue, area.X - 3, area.Y - 3, area.Width + 6, area.Height + 6);
                }

                // Text inside the box: bed number, then patient ID and severity.
                window.DrawText(bed.Number, Color.Black, area.X + 5, area.Y + 6);

                if (bed.IsFree)
                {
                    window.DrawText("free", Color.Black, area.X + 5, area.Y + 24);
                }
                else
                {
                    window.DrawText(bed.Patient.Id, Color.Black, area.X + 5, area.Y + 24);
                    window.DrawText(bed.Patient.Severity.ToString(), Color.Black, area.X + 5, area.Y + 40);
                }
            }
        }
    }

    // Picks the colour for a bed box.
    private Color GetBedColour(Ward ward, Bed bed)
    {
        if (bed.IsFree) return Color.LightGreen;
        if (!ward.CanAdmit(bed.Patient)) return Color.Orange;   // in the wrong ward, waiting to move
        return Color.Salmon;                                    // occupied normally
    }

    // The waiting list, in the order people will be admitted.
    private void DrawWaitingList(Window window)
    {
        double x = 360;
        window.DrawText("Waiting list (next in line at the top)", Color.Black, x, TOP);

        List<Patient> waiting = _hospital.GetWaitingListInOrder();

        if (waiting.Count == 0)
        {
            window.DrawText("Nobody is waiting", Color.Gray, x, TOP + 22);
            return;
        }

        // Each name goes 18 pixels below the one before.
        for (int i = 0; i < waiting.Count; i++)
            window.DrawText($"{i + 1}. {waiting[i].GetSummary()}", Color.Black, x, TOP + 22 + i * 18);
    }

    // Small coloured squares explaining what each colour means.
    private void DrawLegend(Window window)
    {
        double y = GetLegendY();
        window.FillRectangle(Color.LightGreen, LEFT, y, 14, 14);
        window.DrawText("Free", Color.Black, LEFT + 20, y + 3);
        window.FillRectangle(Color.Salmon, LEFT + 80, y, 14, 14);
        window.DrawText("Occupied", Color.Black, LEFT + 100, y + 3);
        window.FillRectangle(Color.Orange, LEFT + 190, y, 14, 14);
        window.DrawText("Awaiting transfer", Color.Black, LEFT + 210, y + 3);
    }

    // The box at the bottom that shows the selected bed's details.
    private void DrawDetails(Window window)
    {
        double y = GetDetailsY();
        window.DrawRectangle(Color.Gray, LEFT, y, 780, 80);

        if (_selectedBed == null)
        {
            window.DrawText("Click a bed to see its details. Close this window to go back to the menu.", Color.Black, LEFT + 10, y + 10);
            return;
        }

        window.DrawText($"Bed {_selectedBed.Number} in {_selectedWard.Name}", Color.Black, LEFT + 10, y + 10);

        if (_selectedBed.IsFree)
        {
            window.DrawText("This bed is free.", Color.Black, LEFT + 10, y + 30);
            return;
        }

        Patient patient = _selectedBed.Patient;
        window.DrawText(patient.GetSummary(), Color.Black, LEFT + 10, y + 30);
        window.DrawText($"Arrived {patient.ArrivalTime:dd/MM/yyyy HH:mm}", Color.Black, LEFT + 10, y + 48);

        if (!_selectedWard.CanAdmit(patient))
            window.DrawText("No longer suits this ward. Will be moved when a suitable bed opens.", Color.Black, LEFT + 10, y + 64);
    }
}
