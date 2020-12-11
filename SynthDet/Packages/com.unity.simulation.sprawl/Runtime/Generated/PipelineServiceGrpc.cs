// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: sprawl/protos/pipeline_service.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Sprawl.Proto {
  public static partial class PipelineService
  {
    static readonly string __ServiceName = "sprawl.PipelineService";

    static readonly grpc::Marshaller<global::Sprawl.Proto.CallPipelineRequest> __Marshaller_sprawl_CallPipelineRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Sprawl.Proto.CallPipelineRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Sprawl.Proto.CallPipelineResponse> __Marshaller_sprawl_CallPipelineResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Sprawl.Proto.CallPipelineResponse.Parser.ParseFrom);

    static readonly grpc::Method<global::Sprawl.Proto.CallPipelineRequest, global::Sprawl.Proto.CallPipelineResponse> __Method_CallPipeline = new grpc::Method<global::Sprawl.Proto.CallPipelineRequest, global::Sprawl.Proto.CallPipelineResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CallPipeline",
        __Marshaller_sprawl_CallPipelineRequest,
        __Marshaller_sprawl_CallPipelineResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Sprawl.Proto.PipelineServiceReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of PipelineService</summary>
    [grpc::BindServiceMethod(typeof(PipelineService), "BindService")]
    public abstract partial class PipelineServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Sprawl.Proto.CallPipelineResponse> CallPipeline(global::Sprawl.Proto.CallPipelineRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for PipelineService</summary>
    public partial class PipelineServiceClient : grpc::ClientBase<PipelineServiceClient>
    {
      /// <summary>Creates a new client for PipelineService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public PipelineServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for PipelineService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public PipelineServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected PipelineServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected PipelineServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Sprawl.Proto.CallPipelineResponse CallPipeline(global::Sprawl.Proto.CallPipelineRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CallPipeline(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Sprawl.Proto.CallPipelineResponse CallPipeline(global::Sprawl.Proto.CallPipelineRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CallPipeline, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Sprawl.Proto.CallPipelineResponse> CallPipelineAsync(global::Sprawl.Proto.CallPipelineRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CallPipelineAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Sprawl.Proto.CallPipelineResponse> CallPipelineAsync(global::Sprawl.Proto.CallPipelineRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CallPipeline, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override PipelineServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new PipelineServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(PipelineServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_CallPipeline, serviceImpl.CallPipeline).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, PipelineServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_CallPipeline, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Sprawl.Proto.CallPipelineRequest, global::Sprawl.Proto.CallPipelineResponse>(serviceImpl.CallPipeline));
    }

  }
}
#endregion