using System;
using System.Linq;
using System.Windows.Forms;
using EFSmartBusinessPro.Datos;
using EFSmartBusinessPro.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EFSmartBusinessPro
{
    public partial class FrmProductos : Form
    {
        private int? productoSeleccionadoId = null;

        public FrmProductos()
        {
            InitializeComponent();

            // Eventos del formulario
            Load += FrmProductos_Load;
            dgvProductos.CellClick += dgvProductos_CellClick;

            btnGuardar.Click += btnGuardar_Click;
            btnActualizar.Click += btnActualizar_Click;
            btnEliminar.Click += btnEliminar_Click;
            btnBuscar.Click += btnBuscar_Click;
            btnNuevo.Click += btnNuevo_Click;

            dgvProductos.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvProductos.MultiSelect = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            cmbCategoria.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbProveedor.DropDownStyle =
                ComboBoxStyle.DropDownList;
        }

        private void FrmProductos_Load(object? sender, EventArgs e)
        {
            try
            {
                CargarCategorias();
                CargarProveedores();
                CargarProductos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // CONSULTAR: cargar categorías disponibles
        private void CargarCategorias()
        {
            using var contexto = new SmartBusinessContext();

            var categorias = contexto.Categorias
                .AsNoTracking()
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.CategoriaId,
                    c.Nombre
                })
                .ToList();

            cmbCategoria.DataSource = categorias;
            cmbCategoria.DisplayMember = "Nombre";
            cmbCategoria.ValueMember = "CategoriaId";
            cmbCategoria.SelectedIndex = -1;
        }

        // CONSULTAR: cargar proveedores disponibles
        private void CargarProveedores()
        {
            using var contexto = new SmartBusinessContext();

            var proveedores = contexto.Proveedores
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.ProveedorId,
                    p.Nombre
                })
                .ToList();

            cmbProveedor.DataSource = proveedores;
            cmbProveedor.DisplayMember = "Nombre";
            cmbProveedor.ValueMember = "ProveedorId";
            cmbProveedor.SelectedIndex = -1;
        }

        // READ: mostrar productos activos
        private void CargarProductos(string filtro = "")
        {
            using var contexto = new SmartBusinessContext();

            var consulta = contexto.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(filtro));
            }

            var productos = consulta
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.ProductoId,
                    p.Nombre,
                    p.Precio,
                    p.Stock,
                    Categoria = p.Categoria.Nombre,
                    Proveedor = p.Proveedor.Nombre,
                    p.FechaIngreso,
                    p.Activo
                })
                .ToList();

            dgvProductos.DataSource = productos;

            if (dgvProductos.Columns["ProductoId"] != null)
            {
                dgvProductos.Columns["ProductoId"]
                    .Visible = false;
            }
        }

        // CREATE: registrar un producto
        private void btnGuardar_Click(
            object? sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                using var contexto = new SmartBusinessContext();

                var producto = new Producto
                {
                    Nombre = txtNombre.Text.Trim(),
                    Precio = decimal.Parse(txtPrecio.Text),
                    Stock = int.Parse(txtStock.Text),
                    FechaIngreso = dtpFechaIngreso.Value.Date,
                    Activo = chkActivo.Checked,
                    CategoriaId = Convert.ToInt32(
                        cmbCategoria.SelectedValue),
                    ProveedorId = Convert.ToInt32(
                        cmbProveedor.SelectedValue)
                };

                contexto.Productos.Add(producto);
                contexto.SaveChanges();

                MessageBox.Show(
                    "Producto registrado correctamente.",
                    "SmartBusinessPro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarProductos();
                LimpiarFormulario();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "No fue posible guardar el producto. " +
                    "Verifique las relaciones y restricciones " +
                    "de la base de datos.",
                    "Error al guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // UPDATE: modificar el producto seleccionado
        private void btnActualizar_Click(
            object? sender, EventArgs e)
        {
            if (productoSeleccionadoId == null)
            {
                MessageBox.Show(
                    "Seleccione un producto de la tabla.");
                return;
            }

            if (!ValidarFormulario())
                return;

            try
            {
                using var contexto = new SmartBusinessContext();

                var producto = contexto.Productos
                    .FirstOrDefault(p =>
                        p.ProductoId ==
                        productoSeleccionadoId.Value);

                if (producto == null)
                {
                    MessageBox.Show(
                        "El producto ya no existe.");
                    CargarProductos();
                    LimpiarFormulario();
                    return;
                }

                producto.Nombre = txtNombre.Text.Trim();
                producto.Precio = decimal.Parse(txtPrecio.Text);
                producto.Stock = int.Parse(txtStock.Text);
                producto.FechaIngreso =
                    dtpFechaIngreso.Value.Date;
                producto.Activo = chkActivo.Checked;
                producto.CategoriaId = Convert.ToInt32(
                    cmbCategoria.SelectedValue);
                producto.ProveedorId = Convert.ToInt32(
                    cmbProveedor.SelectedValue);

                // La entidad está siendo seguida por el contexto.
                contexto.SaveChanges();

                MessageBox.Show(
                    "Producto actualizado correctamente.");

                CargarProductos();
                LimpiarFormulario();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "No fue posible actualizar el producto. " +
                    "Verifique los datos y sus relaciones.");
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // DELETE LÓGICO: desactivar el producto
        private void btnEliminar_Click(
            object? sender, EventArgs e)
        {
            if (productoSeleccionadoId == null)
            {
                MessageBox.Show(
                    "Seleccione un producto para desactivarlo.");
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Desea desactivar el producto seleccionado?",
                "Confirmar operación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                using var contexto = new SmartBusinessContext();

                var producto = contexto.Productos
                    .FirstOrDefault(p =>
                        p.ProductoId ==
                        productoSeleccionadoId.Value);

                if (producto == null)
                {
                    MessageBox.Show(
                        "El producto ya no existe.");
                    CargarProductos();
                    LimpiarFormulario();
                    return;
                }

                producto.Activo = false;
                contexto.SaveChanges();

                MessageBox.Show(
                    "El producto fue desactivado.");

                CargarProductos();
                LimpiarFormulario();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show(
                    "No fue posible desactivar el producto.");
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // Seleccionar una fila y cargar sus datos
        private void dgvProductos_CellClick(
            object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var fila = dgvProductos.Rows[e.RowIndex];

            productoSeleccionadoId = Convert.ToInt32(
                fila.Cells["ProductoId"].Value);

            try
            {
                using var contexto = new SmartBusinessContext();

                var producto = contexto.Productos
                    .AsNoTracking()
                    .FirstOrDefault(p =>
                        p.ProductoId ==
                        productoSeleccionadoId.Value);

                if (producto == null)
                    return;

                txtNombre.Text = producto.Nombre;
                txtPrecio.Text =
                    producto.Precio.ToString("0.00");
                txtStock.Text = producto.Stock.ToString();

                dtpFechaIngreso.Value = producto.FechaIngreso;
                chkActivo.Checked = producto.Activo;

                cmbCategoria.SelectedValue =
                    producto.CategoriaId;

                cmbProveedor.SelectedValue =
                    producto.ProveedorId;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // Búsqueda por nombre
        private void btnBuscar_Click(
            object? sender, EventArgs e)
        {
            try
            {
                CargarProductos(txtBuscar.Text.Trim());
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show(
                    "Ingrese el nombre del producto.");
                txtNombre.Focus();
                return false;
            }

            if (!decimal.TryParse(
                txtPrecio.Text, out decimal precio) ||
                precio < 0)
            {
                MessageBox.Show(
                    "Ingrese un precio válido y no negativo.");
                txtPrecio.Focus();
                return false;
            }

            if (!int.TryParse(
                txtStock.Text, out int stock) ||
                stock < 0)
            {
                MessageBox.Show(
                    "Ingrese una existencia entera no negativa.");
                txtStock.Focus();
                return false;
            }

            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show(
                    "Seleccione una categoría.");
                cmbCategoria.Focus();
                return false;
            }

            if (cmbProveedor.SelectedValue == null)
            {
                MessageBox.Show(
                    "Seleccione un proveedor.");
                cmbProveedor.Focus();
                return false;
            }

            return true;
        }

        private void btnNuevo_Click(
            object? sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            productoSeleccionadoId = null;

            txtNombre.Clear();
            txtPrecio.Clear();
            txtStock.Clear();

            cmbCategoria.SelectedIndex = -1;
            cmbProveedor.SelectedIndex = -1;

            dtpFechaIngreso.Value = DateTime.Today;
            chkActivo.Checked = true;

            dgvProductos.ClearSelection();
            txtNombre.Focus();
        }

        private void MostrarError(Exception ex)
        {
            MessageBox.Show(
                "Se produjo un error al procesar la operación.\n" +
                ex.Message,
                "SmartBusinessPro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
