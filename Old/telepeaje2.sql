USE [Telepeaje]
GO
/****** Object:  Table [dbo].[Historial]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Historial](
	[Archivo] [varchar](50) NOT NULL,
	[fecha] [datetime] NOT NULL,
	[Extension] [int] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[lista]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[lista](
	[Tag] [varchar](50) NOT NULL,
	[Tipo] [varchar](2) NOT NULL,
	[Estatus] [varchar](2) NOT NULL,
	[Saldo] [varchar](15) NOT NULL,
	[EstatusResidente] [varchar](2) NOT NULL DEFAULT ('00'),
	[ResidenteComplementario] [varchar](50) NOT NULL DEFAULT ('0000000000000000000000000000000000000000000000000')
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ListaAntifraude]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ListaAntifraude](
	[Tag] [varchar](50) NOT NULL,
	[Tipo] [varchar](2) NOT NULL,
	[Estatus] [varchar](2) NOT NULL,
	[Saldo] [varchar](15) NOT NULL,
	[EstatusResidente] [varchar](2) NOT NULL DEFAULT ('00'),
	[ResidenteComplementario] [varchar](50) NOT NULL DEFAULT ('0000000000000000000000000000000000000000000000000')
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Listanegra]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Listanegra](
	[tag] [varchar](50) NOT NULL,
	[Fecha] [datetime2](0) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ListaNegraHistorico]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ListaNegraHistorico](
	[Tag] [varchar](50) NOT NULL,
	[Fecha] [datetime2](0) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ListaResidentes]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ListaResidentes](
	[Tag] [varchar](50) NOT NULL,
	[Tipo] [varchar](2) NOT NULL,
	[Estatus] [varchar](2) NOT NULL,
	[Saldo] [varchar](15) NOT NULL,
	[EstatusResidente] [varchar](2) NOT NULL,
	[ResidenteComplementario] [varchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[listaTemporal]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[listaTemporal](
	[Tag] [varchar](50) NOT NULL,
	[Tipo] [varchar](2) NOT NULL,
	[Estatus] [varchar](2) NOT NULL,
	[Saldo] [varchar](15) NOT NULL,
	[EstatusResidente] [varchar](2) NOT NULL DEFAULT ('00'),
	[ResidenteComplementario] [varchar](50) NOT NULL DEFAULT ('0000000000000000000000000000000000000000000000000')
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[listaValidaciones]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[listaValidaciones](
	[Tag] [varchar](50) NOT NULL,
	[Tipo] [varchar](2) NOT NULL,
	[Estatus] [varchar](2) NOT NULL,
	[Saldo] [varchar](15) NOT NULL,
	[EstatusResidente] [varchar](2) NOT NULL DEFAULT ('00'),
	[ResidenteComplementario] [varchar](50) NOT NULL DEFAULT ('0000000000000000000000000000000000000000000000000')
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Parametrizable]    Script Date: 07/03/2018 12:44:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Parametrizable](
	[Destino] [varchar](100) NOT NULL,
	[Origen] [varchar](100) NOT NULL,
	[fmt] [varchar](50) NOT NULL,
	[fmtResidentes] [varchar](50) NOT NULL,
	[MontoRegla] [int] NOT NULL,
	[ReglaCruzes] [int] NOT NULL,
	[ReglaTiempoMinutos] [int] NOT NULL,
	[ListbindEXT] [int] NOT NULL DEFAULT ((1)),
	[OrigenResidentes] [varchar](100) NOT NULL,
	[DestinoResidentes] [varchar](100) NOT NULL,
	[DestinoAntifraude] [varchar](50) NOT NULL,
	[DestinoMontominimo] [varchar](100) NOT NULL,
	[Nombre] [varchar](50) NULL,
	[IpServidor] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[ListaResidentes] ADD  DEFAULT ('00') FOR [EstatusResidente]
GO
ALTER TABLE [dbo].[ListaResidentes] ADD  DEFAULT ('0000000000000000000000000000000000000000000000000') FOR [ResidenteComplementario]
GO
