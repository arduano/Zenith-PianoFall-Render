using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMEngine;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTK.Graphics.OpenGL;

namespace PianoFallRender
{
    public class Render : IPluginRender
    {
        public string Name => "PianoFall+";

        public string Description => "3D Physics note simulations";

        public bool Initialized { get; set; }

        public System.Windows.Media.ImageSource PreviewImage { get; set; } = null;

        public bool ManualNoteDelete => true;

        public int NoteCollectorOffset => 0;

        public double LastMidiTimePerTick { get; set; }
        public MidiInfo CurrentMidi { get; set; }

        public double NoteScreenTime => 0;

        public long LastNoteCount { get; private set; }

        public System.Windows.Controls.Control SettingsControl { get; set; } = null;

        CollisionSystem collision = new CollisionSystemSAP();
        World world;

        RenderSettings renderSettings;
        Settings settings;

        Util util;

        int buffer3dtex;
        int buffer3dbuf;
        int buffer3dbufdepth;

        public Render(RenderSettings settings)
        {
            renderSettings = settings;
            this.settings = new Settings();
        }

        public void Dispose()
        {
            util.Dispose();
            Console.WriteLine("Disposed of PianoFallRender");
            Initialized = false;
        }

        public void Init()
        {
            world = new World(collision);
            util = new Util();
            GLUtils.GenFrameBufferTexture3d(renderSettings.width, renderSettings.height, out buffer3dbuf, out buffer3dtex, out buffer3dbufdepth);
            Console.WriteLine("Initialised PianoFallRender");
            Initialized = true;
        }

        public void RenderFrame(FastList<Note> notes, double midiTime, int finalCompositeBuff)
        {
            GL.Enable(EnableCap.Blend);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Always);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer3dbuf);
            GL.Viewport(0, 0, renderSettings.width, renderSettings.height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            long nc = 0;
            foreach(Note n in notes)
            {
                if(n.start < midiTime)
                {
                    nc++;
                    if (n.meta == null)
                    {
                        Shape shape = new BoxShape(1.0f, 2.0f, 3.0f);
                        RigidBody body = new RigidBody(shape);
                        n.meta = body;
                        world.AddBody(body);
                    }
                    else
                    {
                        if (!n.delete && ((RigidBody)n.meta).Position.Y < 100)
                        {
                            n.delete = true;
                            world.RemoveBody(((RigidBody)n.meta));
                        }
                    }
                }
            }
            LastNoteCount = nc;
            //JVector[] corners = new JVector[8];
            //body.BoundingBox.GetCorners(corners);
            world.Step((float)(1.0f / renderSettings.fps), true);

            GL.Disable(EnableCap.Blend);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, finalCompositeBuff);
            GL.BindTexture(TextureTarget.Texture2D, buffer3dtex);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            util.DrawScreenQuad();
        }

        public void SetTrackColors(NoteColor[][] trakcs)
        {
            
        }
    }
}
